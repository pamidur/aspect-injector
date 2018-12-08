using AspectInjector.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace AspectInjector.Analyzer
{
    public static class Extensions
    {
        private static readonly Dictionary<Rule, DiagnosticDescriptor> _descriptorCache = new Dictionary<Rule, DiagnosticDescriptor>();

        public static TNode RemoveTokenKeepTrivia<TNode>(this TNode node, SyntaxToken token)
            where TNode : SyntaxNode
        {
            var next = token.GetNextToken();

            var newnode = node.ReplaceTokens(new[] { token, next }, (o, r) => o == token ? SyntaxFactory.Token(SyntaxKind.None) : next.WithLeadingTrivia(token.LeadingTrivia));

            return newnode;
        }


        public static DiagnosticDescriptor AsDescriptor(this Rule rule)
        {
            if (!_descriptorCache.TryGetValue(rule, out var descriptor))
                _descriptorCache[rule] = descriptor = new DiagnosticDescriptor(rule.Id, rule.Title, rule.Message, "Aspects", (DiagnosticSeverity)rule.Severity, true, rule.Description, rule.HelpLinkUri);

            return descriptor;
        }
    }
}
