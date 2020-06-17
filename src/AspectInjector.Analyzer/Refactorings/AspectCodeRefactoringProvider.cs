using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace AspectInjector.Analyzer.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AspectCodeRefactoringProvider))]
    public class AspectCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var root = await context.Document
                        .GetSyntaxRootAsync(context.CancellationToken)
                        .ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var @class = node as ClassDeclarationSyntax;
            if (@class == null)
                return;

            var attr = semanticModel.GetDeclaredSymbol(@class).GetAspectAttribute();

            if (attr == null)
                return;

            RegisterRefactoring(context, @class);
        }

        protected void RegisterRefactoring(CodeRefactoringContext context, ClassDeclarationSyntax @class)
        {
            context.RegisterRefactoring(CodeAction.Create(
                "Add 'Before' advice to this aspect.",
                ct =>
                CreateAdvice(context, @class, Samples.Advices.Before, ct)));
            context.RegisterRefactoring(CodeAction.Create(
                "Add 'After' advice to this aspect.",
                ct =>
                CreateAdvice(context, @class, Samples.Advices.After, ct)));
            context.RegisterRefactoring(CodeAction.Create(
                "Add 'Around' advice to this aspect.",
                ct =>
                CreateAdvice(context, @class, Samples.Advices.Around, ct)));
        }

        private async Task<Document> CreateAdvice(CodeRefactoringContext context, ClassDeclarationSyntax @class, (MethodDeclarationSyntax Method, UsingDirectiveSyntax[] Usings) advice, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newClass = @class.AddMembers(advice.Method);

            root = root.ReplaceNode(@class, newClass);
            root = root.WithUpdatedUsings(advice.Usings);

            return context.Document.WithSyntaxRoot(root);
        }
    }
}
