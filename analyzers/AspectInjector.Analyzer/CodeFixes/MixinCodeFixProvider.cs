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
    public class MixinCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(EffectRules.MixinSupportsOnlyAspectInterfaces.Id); }
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            if (diagnostic.Id == EffectRules.MixinSupportsOnlyAspectInterfaces.Id)
                context.RegisterCodeFix(CodeAction.Create(
                    title: $"Add interface '{diagnostic.Properties[WellKnown.MixinTypeProperty]}' to Aspect",
                    createChangedDocument: c => ImplementInterface(context.Document, diagnostic, c),
                    equivalenceKey: diagnostic.Id),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> ImplementInterface(Document document, Diagnostic diagnostic, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var typeDecl = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            var mixinType = diagnostic.Properties[WellKnown.MixinTypeProperty];

            var iface = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(mixinType));

            var newclass = typeDecl
                .WithBaseList((typeDecl.BaseList ?? SyntaxFactory.BaseList()).AddTypes(iface).WithTrailingTrivia(typeDecl.BaseList?.GetTrailingTrivia() ?? typeDecl.Identifier.TrailingTrivia))
                .WithIdentifier(typeDecl.Identifier.WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")));

            var newroot = root.ReplaceNode(typeDecl, newclass);

            return document.WithSyntaxRoot(newroot);
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
