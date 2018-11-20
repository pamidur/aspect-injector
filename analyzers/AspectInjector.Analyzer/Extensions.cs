using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AspectInjector.Analyzer
{
    public static class Extensions
    {
        public static TNode RemoveTokenKeepTrivia<TNode>(this TNode node, SyntaxToken token)
            where TNode : SyntaxNode
        {
            var next = token.GetNextToken();

            var newnode = node.ReplaceTokens(new[] { token, next }, (o, r) => o == token ? SyntaxFactory.Token(SyntaxKind.None) : next.WithLeadingTrivia(token.LeadingTrivia));

            return newnode;
        }
    }
}
