using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AspectInjector.Analyzer
{
    public static class SyntaxBasicBlocks
    {
        public static class Namespaces
        {
            public static readonly UsingDirectiveSyntax System = UsingDirective(IdentifierName("System"));
            public static readonly UsingDirectiveSyntax SystemReflection = UsingDirective(IdentifierName("System.Reflection"));
            public static readonly UsingDirectiveSyntax SystemLinq = UsingDirective(IdentifierName("System.Linq"));
        }

        public static class Types
        {
            public static readonly ArrayTypeSyntax ObjectArray = ArrayType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())));
            public static readonly ArrayTypeSyntax AttributeArray = ArrayType(IdentifierName("Attribute"))
                .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())));
        }
    }
}
