using AspectInjector.Analyzer.Mixin;
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

namespace AspectInjector.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AspectInjectorAnalyzerCodeFixProvider)), Shared]
    public class AspectInjectorAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Rules.AspectShouldImplementMixin.Id, Rules.MixinShouldBePartOfAspect.Id); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            if (diagnostic.Id == Rules.AspectShouldImplementMixin.Id)
                context.RegisterCodeFix(CodeAction.Create(
                    title: $"Add interface '{diagnostic.Properties[MixinAttributeAnalyzer.MinixTypeProperty]}' to Aspect",
                    createChangedDocument: c => ImplementInterface(context.Document, diagnostic, c)),
                diagnostic);
        }

        private async Task<Document> ImplementInterface(Document document, Diagnostic diagnostic, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var typeDecl = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var mixinType = diagnostic.Properties[MixinAttributeAnalyzer.MinixTypeProperty];

            var iface = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(mixinType));

            var newclass = typeDecl
                .WithBaseList((typeDecl.BaseList ?? SyntaxFactory.BaseList()).AddTypes(iface).WithTrailingTrivia(typeDecl.BaseList?.GetTrailingTrivia() ?? typeDecl.Identifier.TrailingTrivia))
                .WithIdentifier(typeDecl.Identifier.WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")));

            var newroot = root.ReplaceNode(typeDecl, newclass);

            return document.WithSyntaxRoot(newroot);
        }
    }
}
