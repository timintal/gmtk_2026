using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EcsGenerators
{
    /// <summary>
    /// Adds [System.Serializable] to StaticEcs component/tag/event structs so StaticEcs-Unity
    /// providers ([SerializeReference] fields) don't raise the "missing [Serializable]" warning.
    ///
    /// UnityEngine.MakeSerializable is NOT usable here: its constructor throws for value types,
    /// and ECS components are structs. The only way to attach [Serializable] from a generator is
    /// through a partial declaration, so eligible structs must be declared 'partial'.
    ///
    /// Runs during compilation off the current syntax trees, so it never goes stale the way a
    /// pre-written registry file can (a rename is reflected in the same compile that consumes it).
    /// </summary>
    [Generator]
    public sealed class MakeSerializableSourceGenerator : IIncrementalGenerator
    {
        const string ComponentOrTagInterface = "FFS.Libraries.StaticEcs.IComponentOrTag";
        const string EventInterface = "FFS.Libraries.StaticEcs.IEvent";
        const string NonSerializableInterface = "FFS.Libraries.StaticEcs.INonSerializable";
        const string SerializableAttribute = "System.SerializableAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var blocks = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is StructDeclarationSyntax s
                        && s.BaseList is { Types.Count: > 0 }
                        && s.Modifiers.Any(SyntaxKind.PartialKeyword),
                    transform: static (ctx, _) => BuildBlock(ctx))
                .Where(static block => block is not null)
                .Collect();

            context.RegisterSourceOutput(blocks, static (spc, items) => Emit(spc, items!));
        }

        static string? BuildBlock(GeneratorSyntaxContext ctx)
        {
            if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
                return null;

            var isEcsType = false;
            foreach (var iface in symbol.AllInterfaces)
            {
                var full = iface.OriginalDefinition.ToDisplayString();
                if (full == NonSerializableInterface)
                    return null;
                if (full == ComponentOrTagInterface || full == EventInterface)
                    isEcsType = true;
            }

            if (!isEcsType)
                return null;

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == SerializableAttribute)
                    return null;
            }

            return Render(symbol);
        }

        // Rebuilds the (possibly nested) declaration as partial with [Serializable] on the leaf.
        static string Render(INamedTypeSymbol symbol)
        {
            var containers = new List<INamedTypeSymbol>();
            for (var t = symbol.ContainingType; t is not null; t = t.ContainingType)
                containers.Add(t);
            containers.Reverse();

            var sb = new StringBuilder();
            var indent = "";

            var ns = symbol.ContainingNamespace;
            var hasNamespace = ns is { IsGlobalNamespace: false };
            if (hasNamespace)
            {
                sb.Append("namespace ").Append(ns!.ToDisplayString()).AppendLine();
                sb.AppendLine("{");
                indent = "    ";
            }

            foreach (var container in containers)
            {
                sb.Append(indent).Append("partial ").Append(Keyword(container))
                    .Append(' ').Append(container.Name).AppendLine();
                sb.Append(indent).AppendLine("{");
                indent += "    ";
            }

            sb.Append(indent).AppendLine("[System.Serializable]");
            sb.Append(indent).Append("partial ").Append(Keyword(symbol))
                .Append(' ').Append(symbol.Name).AppendLine(" { }");

            for (var i = 0; i < containers.Count; i++)
            {
                indent = indent.Substring(0, indent.Length - 4);
                sb.Append(indent).AppendLine("}");
            }

            if (hasNamespace)
                sb.AppendLine("}");

            return sb.ToString();
        }

        static string Keyword(INamedTypeSymbol symbol)
        {
            if (symbol.IsRecord)
                return symbol.IsValueType ? "record struct" : "record";
            return symbol.IsValueType ? "struct" : "class";
        }

        static void Emit(SourceProductionContext spc, ImmutableArray<string> rawBlocks)
        {
            if (rawBlocks.IsDefaultOrEmpty)
                return;

            var blocks = new SortedSet<string>(System.StringComparer.Ordinal);
            foreach (var block in rawBlocks)
                blocks.Add(block);

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine();
            foreach (var block in blocks)
                sb.AppendLine(block);

            spc.AddSource("MakeSerializable.g.cs", sb.ToString());
        }
    }
}
