using AspectInjector.Broker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace AspectInjector.Analyzer
{
    internal class UsingComparer : IEqualityComparer<UsingDirectiveSyntax>
    {
        private UsingComparer() { }
        public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y) => x.Name.IsEquivalentTo(y.Name, true);
        public int GetHashCode(UsingDirectiveSyntax obj) => 1;

        public static readonly UsingComparer Instance = new UsingComparer();
    }

    public static class RoslynExtensions
    {
        public static SyntaxNode WithUpdatedUsings(this SyntaxNode node, UsingDirectiveSyntax[] usings)
        {
            usings = usings.Where(u => u != null).Distinct(UsingComparer.Instance).ToArray();
            if (!usings.Any())
                return node;

            var existingUsings = node.DescendantNodes(s => s is NamespaceDeclarationSyntax || s is CompilationUnitSyntax).OfType<UsingDirectiveSyntax>().ToArray();
            if (existingUsings.Length == 0)
            {
                node = node.InsertNodesBefore(node.ChildNodes().First(), List(usings));
            }
            else
            {
                var newUsings = usings.Except(existingUsings, UsingComparer.Instance);
                node = node.InsertNodesAfter(existingUsings.Last(), List(newUsings));
            }

            return node;
        }

        public static MethodDeclarationSyntax WithAdviceAttribute(this MethodDeclarationSyntax method, Kind kind)
        {
            return method.WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(IdentifierName(nameof(Advice)))
                                .WithArgumentList(
                                    AttributeArgumentList(
                                        SingletonSeparatedList(
                                            AttributeArgument(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(nameof(Kind)),
                                                    IdentifierName(kind.ToString()))))))))));
        }

        public static AttributeData GetAspectAttribute(this ISymbol symbol)
        {
            if (symbol == null) return null;
            var attr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType);
            return attr;
        }

        public static AttributeData GetAdviceAttribute(this ISymbol method)
        {
            if (method == null) return null;
            var attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceType);
            return attr;
        }

        public static IReadOnlyList<AttributeData> GetMixinAttributes(this ISymbol symbol)
        {
            if (symbol == null) return new AttributeData[] { };
            var attrs = symbol.GetAttributes().Where(a => a.AttributeClass.ToDisplayString() == WellKnown.MixinType).ToArray();
            return attrs;
        }
    }
}
