using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EcsGenerators
{
    /// <summary>
    /// Emits a global-namespace GeneratedSystemRegistrar that registers every concrete
    /// FFS.Libraries.StaticEcs.ISystem and Code.Ecs.IFeature class in the compilation with VContainer.
    ///
    /// A type is registered when it is a non-abstract, non-static, non-generic class implementing the
    /// target interface, is reachable from assembly scope, and is not annotated with
    /// [SkipSystemRegistration]. Abstract bases and open generics are skipped.
    ///
    /// Nothing is emitted when the compilation has no registrable types or does not reference VContainer.
    /// Only types declared in the compilation are seen; types from referenced assemblies or produced by
    /// other source generators must be registered by hand.
    ///
    /// The emitted class is partial, so extra registrations can be added in a hand-written partial.
    /// </summary>
    [Generator]
    public sealed class SystemRegistrarGenerator : IIncrementalGenerator
    {
        const string SystemInterface = "FFS.Libraries.StaticEcs.ISystem";
        const string FeatureInterface = "Code.Ecs.IFeature";
        const string SkipAttribute = "SkipSystemRegistrationAttribute";
        const string ContainerBuilderMetadataName = "VContainer.IContainerBuilder";

        enum RegistrationKind : byte
        {
            System,
            Feature,
        }

        readonly struct RegistrationEntry
        {
            public readonly string FullyQualifiedName;
            public readonly RegistrationKind Kind;

            public RegistrationEntry(string fullyQualifiedName, RegistrationKind kind)
            {
                FullyQualifiedName = fullyQualifiedName;
                Kind = kind;
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var entries = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax c && c.BaseList is { Types.Count: > 0 },
                    transform: static (ctx, _) => Analyze(ctx))
                .Where(static entry => entry is not null)
                .Collect();

            context.RegisterSourceOutput(entries, static (spc, rawEntries) => Emit(spc, rawEntries));
        }

        static RegistrationEntry? Analyze(GeneratorSyntaxContext ctx)
        {
            if (ctx.SemanticModel.Compilation.GetTypeByMetadataName(ContainerBuilderMetadataName) is null)
                return null;

            if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
                return null;
            if (symbol.TypeKind != TypeKind.Class || symbol.IsAbstract || symbol.IsStatic)
                return null;
            if (!IsReachableFromAssemblyScope(symbol))
                return null;
            if (HasSkipAttribute(symbol))
                return null;

            if (ImplementsInterface(symbol, SystemInterface))
                return new RegistrationEntry(
                    symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    RegistrationKind.System);

            if (ImplementsInterface(symbol, FeatureInterface))
                return new RegistrationEntry(
                    symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    RegistrationKind.Feature);

            return null;
        }

        static bool ImplementsInterface(INamedTypeSymbol symbol, string interfaceMetadataName)
        {
            foreach (var iface in symbol.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == interfaceMetadataName)
                    return true;
            }
            return false;
        }

        static bool HasSkipAttribute(INamedTypeSymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == SkipAttribute)
                    return true;
            }
            return false;
        }

        static bool IsReachableFromAssemblyScope(INamedTypeSymbol symbol)
        {
            for (var type = symbol; type is not null; type = type.ContainingType)
            {
                if (type.TypeParameters.Length > 0)
                    return false;
                if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                    return false;
            }
            return true;
        }

        static void Emit(SourceProductionContext spc, ImmutableArray<RegistrationEntry?> rawEntries)
        {
            if (rawEntries.IsDefaultOrEmpty)
                return;

            var systems = new List<string>();
            var features = new List<string>();
            var seenSystems = new HashSet<string>(System.StringComparer.Ordinal);
            var seenFeatures = new HashSet<string>(System.StringComparer.Ordinal);

            foreach (var entry in rawEntries)
            {
                switch (entry!.Value.Kind)
                {
                    case RegistrationKind.System when seenSystems.Add(entry.Value.FullyQualifiedName):
                        systems.Add(entry.Value.FullyQualifiedName);
                        break;
                    case RegistrationKind.Feature when seenFeatures.Add(entry.Value.FullyQualifiedName):
                        features.Add(entry.Value.FullyQualifiedName);
                        break;
                }
            }

            if (systems.Count == 0 && features.Count == 0)
                return;

            systems.Sort(System.StringComparer.Ordinal);
            features.Sort(System.StringComparer.Ordinal);

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine();
            sb.AppendLine("using VContainer;");
            sb.AppendLine();
            sb.AppendLine("public static partial class GeneratedSystemRegistrar");
            sb.AppendLine("{");
            sb.Append("    public const int SystemCount = ").Append(systems.Count).AppendLine(";");
            sb.Append("    public const int FeatureCount = ").Append(features.Count).AppendLine(";");
            sb.AppendLine();
            sb.AppendLine("    public static void BindAll("
                          + "global::VContainer.IContainerBuilder builder, "
                          + "global::VContainer.Lifetime lifetime = global::VContainer.Lifetime.Transient)");
            sb.AppendLine("    {");
            sb.AppendLine("        BindSystems(builder, lifetime);");
            sb.AppendLine("        BindFeatures(builder, lifetime);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public static void BindSystems("
                          + "global::VContainer.IContainerBuilder builder, "
                          + "global::VContainer.Lifetime lifetime = global::VContainer.Lifetime.Transient)");
            sb.AppendLine("    {");
            foreach (var name in systems)
                sb.Append("        builder.Register<").Append(name).AppendLine(">(lifetime);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public static void BindFeatures("
                          + "global::VContainer.IContainerBuilder builder, "
                          + "global::VContainer.Lifetime lifetime = global::VContainer.Lifetime.Transient)");
            sb.AppendLine("    {");
            foreach (var name in features)
                sb.Append("        builder.Register<").Append(name).AppendLine(">(lifetime);");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("GeneratedSystemRegistrar.g.cs", sb.ToString());
        }
    }
}
