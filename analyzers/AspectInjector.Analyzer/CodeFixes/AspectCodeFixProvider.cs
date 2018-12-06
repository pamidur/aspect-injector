using AspectInjector.Rules;
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
                AspectRules.AspectMustHaveValidSignature.Id
                );


        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            if (diagnostic.Id == AspectRules.AspectMustHaveValidSignature.Id)
                context.RegisterCodeFix(CodeAction.Create(
                    title: $"Fix Aspect signature",
                    createChangedDocument: c => RemoveModifier(context.Document, diagnostic.Location.SourceSpan.Start, c)),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> RemoveModifier(Document document, int from, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var type = root.FindToken(from).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            var newtype = type;

            var statictoken = newtype.Modifiers.FirstOrDefault(m => m.Kind() == SyntaxKind.StaticKeyword);
            if (statictoken != null)
                newtype = newtype.RemoveTokenKeepTrivia(statictoken);

            var abstracttoken = newtype.Modifiers.FirstOrDefault(m => m.Kind() == SyntaxKind.AbstractKeyword);
            if (abstracttoken != null)
                newtype = newtype.RemoveTokenKeepTrivia(abstracttoken);

            if (newtype.TypeParameterList != null && !newtype.TypeParameterList.Span.IsEmpty)
                newtype = newtype.RemoveNode(newtype.TypeParameterList, SyntaxRemoveOptions.KeepExteriorTrivia);

            return document.WithSyntaxRoot(root.ReplaceNode(type, newtype));
        }
    }
}
