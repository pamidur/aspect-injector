using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspectInjector.Analyzer.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MixinCodeFixProvider)), Shared]
    public class AspectCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(
                Rules.AspectMustNotBeStatic.Id
                );


        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            if (diagnostic.Id == Rules.AspectMustNotBeStatic.Id)
                context.RegisterCodeFix(CodeAction.Create(
                    title: $"Make Aspect non-static",
                    createChangedDocument: c => RemoveModifier(SyntaxKind.StaticKeyword, context.Document, diagnostic.Location.SourceSpan.Start, c)),
                diagnostic);

            if (diagnostic.Id == Rules.AspectMustNotBeAbstract.Id)
                context.RegisterCodeFix(CodeAction.Create(
                    title: $"Make Aspect non-abstract",
                    createChangedDocument: c => RemoveModifier(SyntaxKind.AbstractKeyword, context.Document, diagnostic.Location.SourceSpan.Start, c)),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> RemoveModifier(SyntaxKind modifier, Document document, int from, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var type = root.FindToken(from).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            var token = type.Modifiers.First(m => m.Kind() == modifier);

            var newtype = type.RemoveTokenKeepTrivia(token);

            return document.WithSyntaxRoot(root.ReplaceNode(type, newtype));
        }
    }
}
