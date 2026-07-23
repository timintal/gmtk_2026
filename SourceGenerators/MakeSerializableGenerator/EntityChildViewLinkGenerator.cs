using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MakeSerializableGenerator
{
    /// <summary>
    /// For every concrete class deriving from Code.Common.View.EntityChildView, generates a link
    /// component: [Serializable] public partial struct {Name}Link : IComponent { public {Name} Value; }
    ///
    /// It additionally generates Bind/Unbind overrides (Set the link on bind, Delete on unbind) ONLY
    /// when the class does not already declare its own Bind/Unbind. A class with hand-written
    /// Bind/Unbind manages binding itself and still gets the link struct — it just wires it manually.
    ///
    /// Optional hooks PostBind()/PostUnbind() (parameterless) are called from the generated
    /// Bind/Unbind when present. Auto Bind/Unbind require the class to be 'partial'; if it isn't, a
    /// diagnostic points that out, but the link struct is emitted regardless.
    /// </summary>
    [Generator]
    public sealed class EntityChildViewLinkGenerator : IIncrementalGenerator
    {
        const string BaseTypeName = "Code.Common.View.EntityChildView";

        static readonly DiagnosticDescriptor NotPartial = new(
            id: "FFSVIEW001",
            title: "EntityChildView subclass is not partial",
            messageFormat: "'{0}' derives from EntityChildView but is not 'partial'; add 'partial' to auto-generate its Bind/Unbind (the link component is generated regardless)",
            category: "StaticEcs",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        sealed class Model
        {
            public string? Namespace;
            public string ClassName = "";
            public string FullyQualified = "";
            public bool IsPartial;
            public bool HasManualBind;
            public bool HasPostBind;
            public bool HasPostUnbind;
            public Location? Location;

            public override bool Equals(object? obj) =>
                obj is Model m && m.Namespace == Namespace && m.ClassName == ClassName
                && m.FullyQualified == FullyQualified && m.IsPartial == IsPartial
                && m.HasManualBind == HasManualBind
                && m.HasPostBind == HasPostBind && m.HasPostUnbind == HasPostUnbind;

            public override int GetHashCode() =>
                (FullyQualified, IsPartial, HasManualBind, HasPostBind, HasPostUnbind).GetHashCode();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var models = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax c && c.BaseList is { Types.Count: > 0 },
                    transform: static (ctx, _) => Analyze(ctx))
                .Where(static m => m is not null)
                .Collect();

            context.RegisterSourceOutput(models, static (spc, items) =>
            {
                foreach (var model in items)
                    Produce(spc, model!);
            });
        }

        static Model? Analyze(GeneratorSyntaxContext ctx)
        {
            if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
                return null;
            if (symbol.IsAbstract)
                return null;

            var derives = false;
            for (var b = symbol.BaseType; b is not null; b = b.BaseType)
            {
                if (b.ToDisplayString() == BaseTypeName)
                {
                    derives = true;
                    break;
                }
            }
            if (!derives)
                return null;

            var hasManualBind = false;
            var hasPostBind = false;
            var hasPostUnbind = false;
            foreach (var member in symbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;
                switch (method.Name)
                {
                    // Hand-written Bind/Unbind => the class manages binding itself; skip the overrides.
                    case "Bind" or "Unbind":
                        hasManualBind = true;
                        break;
                    case "PostBind" when method.Parameters.Length == 0:
                        hasPostBind = true;
                        break;
                    case "PostUnbind" when method.Parameters.Length == 0:
                        hasPostUnbind = true;
                        break;
                }
            }

            var decl = (ClassDeclarationSyntax)ctx.Node;
            var ns = symbol.ContainingNamespace;

            return new Model
            {
                Namespace = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : null,
                ClassName = symbol.Name,
                FullyQualified = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                IsPartial = decl.Modifiers.Any(SyntaxKind.PartialKeyword),
                HasManualBind = hasManualBind,
                HasPostBind = hasPostBind,
                HasPostUnbind = hasPostUnbind,
                Location = decl.Identifier.GetLocation(),
            };
        }

        static void Produce(SourceProductionContext spc, Model model)
        {
            var link = model.ClassName + "Link";
            var indent = model.Namespace is null ? "" : "    ";

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine();
            if (model.Namespace is not null)
            {
                sb.Append("namespace ").Append(model.Namespace).AppendLine();
                sb.AppendLine("{");
            }

            // The link component is always generated, regardless of manual Bind/Unbind or partial-ness.
            sb.Append(indent).AppendLine("[global::System.Serializable]");
            sb.Append(indent).Append("public partial struct ").Append(link)
                .Append(" : global::FFS.Libraries.StaticEcs.IComponent { public ")
                .Append(model.FullyQualified).AppendLine(" Value; }");

            // Auto Bind/Unbind only when the class doesn't provide its own.
            var emitOverrides = !model.HasManualBind && model.IsPartial;
            if (emitOverrides)
            {
                sb.AppendLine();
                sb.Append(indent).Append("partial class ").AppendLine(model.ClassName);
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).AppendLine("    public override void Bind(global::W.Entity entity)");
                sb.Append(indent).AppendLine("    {");
                sb.Append(indent).AppendLine("        base.Bind(entity);");
                sb.Append(indent).Append("        entity.Set(new ").Append(link).AppendLine(" { Value = this });");
                if (model.HasPostBind)
                    sb.Append(indent).AppendLine("        PostBind();");
                sb.Append(indent).AppendLine("    }");
                sb.AppendLine();
                sb.Append(indent).AppendLine("    public override void Unbind()");
                sb.Append(indent).AppendLine("    {");
                sb.Append(indent).AppendLine("        if (_entity.TryUnpack<global::WT>(out var __linkEntity))");
                sb.Append(indent).Append("            __linkEntity.Delete<").Append(link).AppendLine(">();");
                // PostUnbind runs while the entity is still bound (before base clears it).
                if (model.HasPostUnbind)
                    sb.Append(indent).AppendLine("        PostUnbind();");
                sb.Append(indent).AppendLine("        base.Unbind();");
                sb.Append(indent).AppendLine("    }");
                sb.Append(indent).AppendLine("}");
            }

            if (model.Namespace is not null)
                sb.AppendLine("}");

            var hint = model.FullyQualified.Replace("global::", "").Replace('.', '_') + ".EntityChildViewLink.g.cs";
            spc.AddSource(hint, sb.ToString());

            // Nudge only when we *wanted* to emit overrides but couldn't due to a missing 'partial'.
            if (!model.HasManualBind && !model.IsPartial)
                spc.ReportDiagnostic(Diagnostic.Create(NotPartial, model.Location, model.ClassName));
        }
    }
}
