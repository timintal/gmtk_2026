using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EcsGenerators
{
    [Generator]
    public sealed class PersistentDataSourceGenerator : IIncrementalGenerator
    {
        private const string PersistentDataAttribute =
            "_Game.Infrastructure.Persistence.PersistentDataAttribute";
        private const string PersistentFieldAttribute =
            "_Game.Infrastructure.Persistence.PersistentFieldAttribute";
        private const string PersistentDataBase =
            "_Game.Infrastructure.Persistence.PersistentDataBase";
        private const string PersistentDataInterface =
            "_Game.Infrastructure.Persistence.IPersistentData";
        private static readonly DiagnosticDescriptor MustBePartial = new(
            "PD001",
            "Persistent data must be partial",
            "Persistent data class '{0}' must be declared partial",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor DuplicateKey = new(
            "PD002",
            "Duplicate persistent data key",
            "Persistent key '{0}' is used by both '{1}' and '{2}'",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor InvalidField = new(
            "PD003",
            "Persistent field cannot be generated",
            "Persistent field '{0}' must be an instance, non-readonly field",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor MutableCollection = new(
            "PD004",
            "Collection mutations are not tracked",
            "Persistent field '{0}' is a mutable collection; replacing the property is tracked, but mutations inside the collection are not",
            "Persistence",
            DiagnosticSeverity.Warning,
            true);

        private static readonly DiagnosticDescriptor InvalidDataType = new(
            "PD005",
            "Invalid persistent data type",
            "Persistent data class '{0}' must be non-generic and derive from PersistentDataBase",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor MemberCollision = new(
            "PD006",
            "Generated member name collision",
            "Cannot generate member '{0}' on '{1}' because a member with that name already exists",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        private static readonly DiagnosticDescriptor InvalidKey = new(
            "PD007",
            "Invalid persistent data key",
            "Persistent data class '{0}' must declare a non-empty persistence key",
            "Persistence",
            DiagnosticSeverity.Error,
            true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidates = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (node, _) =>
                        node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                    static (ctx, _) => GetPersistentDataSymbol(ctx))
                .Where(static symbol => symbol is not null)
                .Collect();

            context.RegisterSourceOutput(
                candidates,
                static (productionContext, symbols) => Emit(productionContext, symbols));
        }

        private static INamedTypeSymbol? GetPersistentDataSymbol(GeneratorSyntaxContext context)
        {
            if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol symbol)
            {
                return null;
            }

            return GetAttribute(symbol, PersistentDataAttribute) is null ? null : symbol;
        }

        private static void Emit(
            SourceProductionContext context,
            ImmutableArray<INamedTypeSymbol?> rawSymbols)
        {
            if (rawSymbols.IsDefaultOrEmpty)
            {
                return;
            }

            var symbols = new List<INamedTypeSymbol>();
            foreach (var symbol in rawSymbols)
            {
                if (symbol is null ||
                    symbols.Any(existing => SymbolEqualityComparer.Default.Equals(existing, symbol)))
                {
                    continue;
                }

                symbols.Add(symbol);
            }

            symbols.Sort(static (left, right) =>
                StringComparer.Ordinal.Compare(GetQualifiedName(left), GetQualifiedName(right)));

            var validModels = new List<DataModel>();
            var keys = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

            foreach (var symbol in symbols)
            {
                var model = Analyze(context, symbol);
                if (model is null)
                {
                    continue;
                }

                if (keys.TryGetValue(model.Key, out var existing))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DuplicateKey,
                        symbol.Locations.FirstOrDefault(),
                        model.Key,
                        GetQualifiedName(existing),
                        GetQualifiedName(symbol)));
                    continue;
                }

                keys.Add(model.Key, symbol);
                validModels.Add(model);
                context.AddSource(
                    $"{SanitizeHintName(GetQualifiedName(symbol))}.PersistentData.g.cs",
                    RenderDataType(model));
            }

            if (validModels.Count > 0)
            {
                context.AddSource(
                    "GeneratedPersistentDataRegistrar.g.cs",
                    RenderRegistrar(validModels));
            }
        }

        private static DataModel? Analyze(
            SourceProductionContext context,
            INamedTypeSymbol symbol)
        {
            var dataAttribute = GetAttribute(symbol, PersistentDataAttribute);
            var key = dataAttribute?.ConstructorArguments.FirstOrDefault().Value as string;

            if (string.IsNullOrWhiteSpace(key))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidKey,
                    symbol.Locations.FirstOrDefault(),
                    GetQualifiedName(symbol)));
                return null;
            }

            if (!IsPartial(symbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MustBePartial,
                    symbol.Locations.FirstOrDefault(),
                    GetQualifiedName(symbol)));
                return null;
            }

            if (symbol.TypeParameters.Length > 0 || !DerivesFrom(symbol, PersistentDataBase))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    InvalidDataType,
                    symbol.Locations.FirstOrDefault(),
                    GetQualifiedName(symbol)));
                return null;
            }

            if (symbol.GetMembers("Key").Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MemberCollision,
                    symbol.Locations.FirstOrDefault(),
                    "Key",
                    GetQualifiedName(symbol)));
                return null;
            }

            var fields = new List<FieldModel>();
            foreach (var field in symbol.GetMembers().OfType<IFieldSymbol>())
            {
                var fieldAttribute = GetAttribute(field, PersistentFieldAttribute);
                if (fieldAttribute is null)
                {
                    continue;
                }

                if (field.IsStatic || field.IsReadOnly || field.IsConst)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        InvalidField,
                        field.Locations.FirstOrDefault(),
                        field.Name));
                    continue;
                }

                var explicitName = fieldAttribute.ConstructorArguments.FirstOrDefault().Value as string;
                var propertyName = string.IsNullOrWhiteSpace(explicitName)
                    ? GetPropertyName(field.Name)
                    : explicitName!;

                if (!SyntaxFacts.IsValidIdentifier(propertyName) ||
                    symbol.GetMembers(propertyName).Length > 0 ||
                    fields.Any(existing => existing.PropertyName == propertyName))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MemberCollision,
                        field.Locations.FirstOrDefault(),
                        propertyName,
                        GetQualifiedName(symbol)));
                    continue;
                }

                if (IsMutableCollection(field.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MutableCollection,
                        field.Locations.FirstOrDefault(),
                        field.Name));
                }

                fields.Add(new FieldModel(
                    field.Name,
                    propertyName,
                    field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            }

            return new DataModel(symbol, key!, fields);
        }

        private static string RenderDataType(DataModel model)
        {
            var symbol = model.Symbol;
            var containers = new List<INamedTypeSymbol>();
            for (var containingType = symbol.ContainingType;
                 containingType is not null;
                 containingType = containingType.ContainingType)
            {
                containers.Add(containingType);
            }
            containers.Reverse();

            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();

            var indent = "";
            if (!symbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.Append("namespace ")
                    .Append(symbol.ContainingNamespace.ToDisplayString())
                    .AppendLine();
                builder.AppendLine("{");
                indent = "    ";
            }

            foreach (var container in containers)
            {
                builder.Append(indent)
                    .Append("partial class ")
                    .Append(EscapeIdentifier(container.Name))
                    .AppendLine();
                builder.Append(indent).AppendLine("{");
                indent += "    ";
            }

            builder.Append(indent)
                .Append("partial class ")
                .Append(EscapeIdentifier(symbol.Name))
                .AppendLine();
            builder.Append(indent).AppendLine("{");
            indent += "    ";

            builder.Append(indent)
                .Append("public override string Key => ")
                .Append(ToLiteral(model.Key))
                .AppendLine(";");

            foreach (var field in model.Fields)
            {
                builder.AppendLine();
                builder.Append(indent)
                    .Append("public ")
                    .Append(field.TypeName)
                    .Append(' ')
                    .Append(EscapeIdentifier(field.PropertyName))
                    .AppendLine();
                builder.Append(indent).AppendLine("{");
                builder.Append(indent)
                    .Append("    get => ")
                    .Append(EscapeIdentifier(field.FieldName))
                    .AppendLine(";");
                builder.Append(indent)
                    .Append("    set => SetPersistentField(ref ")
                    .Append(EscapeIdentifier(field.FieldName))
                    .AppendLine(", value);");
                builder.Append(indent).AppendLine("}");
            }

            indent = indent.Substring(0, indent.Length - 4);
            builder.Append(indent).AppendLine("}");

            for (var i = containers.Count - 1; i >= 0; i--)
            {
                indent = indent.Substring(0, indent.Length - 4);
                builder.Append(indent).AppendLine("}");
            }

            if (!symbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        private static string RenderRegistrar(IReadOnlyList<DataModel> models)
        {
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine();
            builder.AppendLine("using VContainer;");
            builder.AppendLine();
            builder.AppendLine("public static class GeneratedPersistentDataRegistrar");
            builder.AppendLine("{");
            builder.AppendLine("    public static void Register(global::VContainer.IContainerBuilder builder)");
            builder.AppendLine("    {");

            foreach (var model in models)
            {
                var typeName = model.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                builder.Append("        builder.Register<")
                    .Append(typeName)
                    .AppendLine(">(global::VContainer.Lifetime.Singleton)");
                builder.AppendLine("            .AsSelf()");
                builder.Append("            .As<global::")
                    .Append(PersistentDataInterface)
                    .AppendLine(">();");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static AttributeData? GetAttribute(ISymbol symbol, string metadataName)
        {
            return symbol.GetAttributes().FirstOrDefault(attribute =>
                attribute.AttributeClass?.ToDisplayString() == metadataName);
        }

        private static bool IsPartial(INamedTypeSymbol symbol)
        {
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is TypeDeclarationSyntax declaration &&
                    !declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DerivesFrom(INamedTypeSymbol symbol, string baseTypeName)
        {
            for (var type = symbol.BaseType; type is not null; type = type.BaseType)
            {
                if (type.ToDisplayString() == baseTypeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMutableCollection(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol)
            {
                return true;
            }

            if (type.SpecialType == SpecialType.System_String)
            {
                return false;
            }

            return type.AllInterfaces.Any(interfaceType =>
                interfaceType.OriginalDefinition.ToDisplayString() is
                    "System.Collections.ICollection" or
                    "System.Collections.Generic.ICollection<T>" or
                    "System.Collections.Generic.IDictionary<TKey, TValue>");
        }

        private static string GetPropertyName(string fieldName)
        {
            var trimmed = fieldName.StartsWith("m_", StringComparison.Ordinal)
                ? fieldName.Substring(2)
                : fieldName.TrimStart('_');

            if (trimmed.Length == 0)
            {
                return fieldName;
            }

            return char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1);
        }

        private static string GetQualifiedName(INamedTypeSymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static string EscapeIdentifier(string identifier)
        {
            return SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None
                ? identifier
                : "@" + identifier;
        }

        private static string ToLiteral(string value)
        {
            return SymbolDisplay.FormatLiteral(value, true);
        }

        private static string SanitizeHintName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            }

            return builder.ToString();
        }

        private sealed class DataModel
        {
            public DataModel(
                INamedTypeSymbol symbol,
                string key,
                IReadOnlyList<FieldModel> fields)
            {
                Symbol = symbol;
                Key = key;
                Fields = fields;
            }

            public INamedTypeSymbol Symbol { get; }
            public string Key { get; }
            public IReadOnlyList<FieldModel> Fields { get; }
        }

        private sealed class FieldModel
        {
            public FieldModel(string fieldName, string propertyName, string typeName)
            {
                FieldName = fieldName;
                PropertyName = propertyName;
                TypeName = typeName;
            }

            public string FieldName { get; }
            public string PropertyName { get; }
            public string TypeName { get; }
        }
    }
}
