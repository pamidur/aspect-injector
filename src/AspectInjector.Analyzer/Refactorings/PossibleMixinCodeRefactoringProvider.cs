using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using AspectInjector.Broker;

namespace AspectInjector.Analyzer.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(PossibleMixinCodeRefactoringProvider))]
    public class PossibleMixinCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var @base = node as BaseTypeSyntax;
            if (@base != null)
            {
                var baseType = semanticModel.GetSymbolInfo(@base.Type).Symbol as ITypeSymbol;
                if (baseType != null && baseType.TypeKind == TypeKind.Interface)
                {
                    var @class = @base.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    if (@class != null)
                    {
                        var classSymbol = semanticModel.GetDeclaredSymbol(@class);
                        var attr = classSymbol?.GetAspectAttribute();
                        if (attr != null)
                        {
                            var currentMixins = classSymbol.GetMixinAttributes();
                            if (!currentMixins.Any(m => m.ConstructorArguments.Length > 0 &&
                                m.ConstructorArguments[0].Value is ITypeSymbol ts &&
                                ts == baseType))
                            {
                                context.RegisterRefactoring(CodeAction.Create(
                                    "Register this interface with Aspect as Mixin.",
                                    ct => AddMixinAttribute(context, @class, @base.Type, ct)));
                            }
                        }
                    }
                }
            }
        }

        private async Task<Document> AddMixinAttribute(CodeRefactoringContext context, ClassDeclarationSyntax @class, TypeSyntax baseType, CancellationToken ct)
        {
            var root = await context.Document.GetSyntaxRootAsync(ct).ConfigureAwait(false);

            var newClass = @class.WithAttributeLists(@class.AttributeLists.Add(
                AttributeList(
                    SeparatedList(new[]{
                        Attribute(IdentifierName(nameof(Mixin)),
                            AttributeArgumentList(SeparatedList(new[]{ AttributeArgument(TypeOfExpression(baseType.WithoutTrivia())) }))
                            )
                    })
                    ).WithAdditionalAnnotations(Formatter.Annotation)
                ));

            root = root.ReplaceNode(@class, newClass);

            return context.Document.WithSyntaxRoot(root);
        }
    }
}
