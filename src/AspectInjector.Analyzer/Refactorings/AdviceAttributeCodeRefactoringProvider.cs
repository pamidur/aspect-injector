using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace AspectInjector.Analyzer.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AdviceAttributeCodeRefactoringProvider))]
    public class AdviceAttributeCodeRefactoringProvider : AdviceCodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var attrSyntax = (node as AttributeSyntax) ?? node.Parent as AttributeSyntax;

            if (attrSyntax != null)
            {
                var attr = semanticModel.GetSymbolInfo(attrSyntax, context.CancellationToken);
                if (attr.Symbol?.ContainingSymbol is INamedTypeSymbol named && named.ToDisplayString() == WellKnown.AdviceType)
                {
                    var method = attrSyntax.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                    if (method != null)
                        await RegisterRefactorings(context, method);
                }
            }
        }
    }
}
