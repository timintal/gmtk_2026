using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EcsGenerators
{
    /// <summary>
    /// For every concrete struct implementing Code.Features.Stats.IStat, generates the boilerplate
    /// that would otherwise be hand-written for each stat:
    ///
    ///   1. A modifier component:  [Serializable] public partial struct {Stat}Modifier : IStatModifier&lt;{Stat}&gt;
    ///      Exposes public 'Additive'/'Multiplicative' FIELDS (Unity-inspector serializable) that back the
    ///      IStatModifier properties via explicit interface implementation. StaticEcs' RegisterAll() picks it up
    ///      automatically because it implements IComponent through IStatModifier.
    ///   2. A reset system:  public sealed class Reset{Stat}System : ResetStatSystem&lt;{Stat}&gt;
    ///   3. An apply system:  public sealed class Apply{Stat}ModifierSystem : ApplyStatModifierSystem&lt;{Stat}, {Stat}Modifier&gt;
    ///
    /// When the compilation that owns the stats also declares Code.Features.Stats.StatsFeature, an
    /// implementing partial for StatsFeature.RegisterGeneratedStatSystems() is emitted that registers every
    /// generated reset/apply pair (reset before apply). If no stats exist, nothing is emitted and the partial
    /// method call in StatsFeature is elided by the compiler.
    ///
    /// The names {Stat}Modifier / Reset{Stat}System / Apply{Stat}ModifierSystem are owned by this generator;
    /// declaring a type with the same name by hand will collide.
    /// </summary>
    [Generator]
    public sealed class StatSystemsGenerator : IIncrementalGenerator
    {
        const string StatInterface = "Code.Features.Stats.IStat";
        const string StatsFeatureType = "Code.Features.Stats.StatsFeature";

        sealed class StatModel
        {
            public string? Namespace;
            public string Name = "";
            public string GlobalStat = ""; // e.g. global::_Game.Features.Enemies.Attack

            public string GlobalNsPrefix => Namespace is null ? "global::" : "global::" + Namespace + ".";
            public string ModifierName => Name + "Modifier";
            public string GlobalModifier => GlobalStat + "Modifier";
            public string ResetSystemName => "Reset" + Name + "System";
            public string ApplySystemName => "Apply" + Name + "ModifierSystem";

            public override bool Equals(object? obj) =>
                obj is StatModel m && m.Namespace == Namespace && m.Name == Name && m.GlobalStat == GlobalStat;

            public override int GetHashCode() => (Namespace, Name, GlobalStat).GetHashCode();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var stats = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is StructDeclarationSyntax s && s.BaseList is { Types.Count: > 0 },
                    transform: static (ctx, _) => Analyze(ctx))
                .Where(static m => m is not null)
                .Collect();

            // True (present) once the compilation declares Code.Features.Stats.StatsFeature.
            var hasFeature = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax c && c.Identifier.ValueText == "StatsFeature",
                    transform: static (ctx, _) =>
                        ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is INamedTypeSymbol s
                        && s.ToDisplayString() == StatsFeatureType)
                .Where(static present => present)
                .Collect();

            context.RegisterSourceOutput(stats.Combine(hasFeature),
                static (spc, pair) => Emit(spc, pair.Left!, !pair.Right.IsDefaultOrEmpty));
        }

        static StatModel? Analyze(GeneratorSyntaxContext ctx)
        {
            if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
                return null;
            if (symbol.IsAbstract || symbol.TypeKind != TypeKind.Struct)
                return null;

            var isStat = false;
            foreach (var iface in symbol.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == StatInterface)
                {
                    isStat = true;
                    break;
                }
            }
            if (!isStat)
                return null;

            var ns = symbol.ContainingNamespace;
            return new StatModel
            {
                Namespace = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : null,
                Name = symbol.Name,
                GlobalStat = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            };
        }

        static void Emit(SourceProductionContext spc, ImmutableArray<StatModel> rawStats, bool hasFeature)
        {
            if (rawStats.IsDefaultOrEmpty)
                return;

            // Dedupe (a stat could be seen via multiple partial declarations).
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            var stats = new List<StatModel>();
            foreach (var stat in rawStats)
            {
                if (seen.Add(stat.GlobalStat))
                    stats.Add(stat);
            }
            stats.Sort(static (a, b) => System.StringComparer.Ordinal.Compare(a.GlobalStat, b.GlobalStat));

            foreach (var stat in stats)
                EmitStat(spc, stat);

            if (hasFeature)
                EmitRegistrar(spc, stats);
        }

        static void EmitStat(SourceProductionContext spc, StatModel stat)
        {
            var indent = stat.Namespace is null ? "" : "    ";
            var iface = "global::Code.Features.Stats.IStatModifier<" + stat.GlobalStat + ">";

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine();
            if (stat.Namespace is not null)
            {
                sb.Append("namespace ").Append(stat.Namespace).AppendLine();
                sb.AppendLine("{");
            }

            sb.Append(indent).AppendLine("[global::System.Serializable]");
            sb.Append(indent).Append("public partial struct ").Append(stat.ModifierName)
                .Append(" : ").AppendLine(iface);
            sb.Append(indent).AppendLine("{");
            sb.Append(indent).AppendLine("    public float Additive;");
            sb.Append(indent).AppendLine("    public float Multiplicative;");
            sb.Append(indent).Append("    float ").Append(iface)
                .AppendLine(".Additive { get => Additive; set => Additive = value; }");
            sb.Append(indent).Append("    float ").Append(iface)
                .AppendLine(".Multiplicative { get => Multiplicative; set => Multiplicative = value; }");
            sb.Append(indent).AppendLine("}");
            sb.AppendLine();

            sb.Append(indent).Append("public sealed class ").Append(stat.ResetSystemName)
                .Append(" : global::Code.Features.Stats.ResetStatSystem<").Append(stat.GlobalStat).AppendLine("> { }");
            sb.AppendLine();

            sb.Append(indent).Append("public sealed class ").Append(stat.ApplySystemName)
                .Append(" : global::Code.Features.Stats.ApplyStatModifierSystem<")
                .Append(stat.GlobalStat).Append(", ").Append(stat.GlobalModifier).AppendLine("> { }");

            if (stat.Namespace is not null)
                sb.AppendLine("}");

            var hint = stat.GlobalStat.Replace("global::", "").Replace('.', '_') + ".StatSystems.g.cs";
            spc.AddSource(hint, sb.ToString());
        }

        static void EmitRegistrar(SourceProductionContext spc, List<StatModel> stats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine();
            sb.AppendLine("namespace Code.Features.Stats");
            sb.AppendLine("{");
            sb.AppendLine("    public partial class StatsFeature");
            sb.AppendLine("    {");
            sb.AppendLine("        partial void RegisterGeneratedStatSystems()");
            sb.AppendLine("        {");
            foreach (var stat in stats)
            {
                sb.Append("            global::GameSys.Add(new ").Append(stat.GlobalNsPrefix)
                    .Append(stat.ResetSystemName).AppendLine("(), StatResetOrder);");
                sb.Append("            global::GameSys.Add(new ").Append(stat.GlobalNsPrefix)
                    .Append(stat.ApplySystemName).AppendLine("(), StatApplyOrder);");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource("StatsFeature.GeneratedRegistration.g.cs", sb.ToString());
        }
    }
}
