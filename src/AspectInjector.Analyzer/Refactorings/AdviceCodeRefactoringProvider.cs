using AspectInjector.Broker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspectInjector.Analyzer.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AdviceCodeRefactoringProvider))]
    public class AdviceCodeRefactoringProvider : CodeRefactoringProvider
    {
        private static readonly ImmutableArray<ParameterSample> _shared = new List<ParameterSample>
        {
            Samples.Parameters.Arguments,
            Samples.Parameters.Instance,
            Samples.Parameters.Name,
            Samples.Parameters.Type,
            Samples.Parameters.Metadata,
            Samples.Parameters.ReturnType,
            Samples.Parameters.Triggers,
        }.ToImmutableArray();

        private static readonly ImmutableArray<ParameterSample> _after = _shared.AddRange(new List<ParameterSample>
        {
            Samples.Parameters.ReturnValue,
        });

        private static readonly ImmutableArray<ParameterSample> _around = _shared.AddRange(new List<ParameterSample>
        {
            Samples.Parameters.Target,
        });

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var node = root.FindNode(context.Span);
            var method = node as MethodDeclarationSyntax;
            if (method != null)
            {
                await RegisterRefactorings(context, method);
            }           
        }

        protected async Task RegisterRefactorings(CodeRefactoringContext context, MethodDeclarationSyntax method)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var attr = methodSymbol.GetAdviceAttribute();

            if (attr == null)
                return;

            var kindArg = attr.ConstructorArguments.FirstOrDefault();

            if (kindArg.IsNull)
                return;

            var kind = (Kind)kindArg.Value;

            var list = _shared;
            if (kind == Kind.After)
                list = _after;
            if (kind == Kind.Around)
                list = _around;

            var currentAttrs = methodSymbol.Parameters.SelectMany(p => p.GetAttributes()).ToImmutableArray();

            foreach (var entry in list)
            {
                if (HasParameter(currentAttrs, entry.Source))
                    continue;

                context.RegisterRefactoring(CodeAction.Create(
                    $"Add '{entry.Source}' parameter to this advice.",
                    ct => AddParameter(context, method, entry, ct)
                ));
            }
        }

        private async Task<Document> AddParameter(CodeRefactoringContext context, MethodDeclarationSyntax method, ParameterSample parameter, CancellationToken ct)
        {
            var root = await context.Document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var newMethod = method.WithParameterList(method.ParameterList.AddParameters(parameter.ParameterSyntax)/*.WithAdditionalAnnotations(Formatter.Annotation)*/);

            root = root.ReplaceNode(method, newMethod);
            root = root.WithUpdatedUsings(new[] { parameter.UsingSyntax });           

            return context.Document.WithSyntaxRoot(root);
        }

        private bool HasParameter(ImmutableArray<AttributeData> attributes, string source)
        {
            return attributes.Any(a => a.ConstructorArguments.Any(ca => ((Source)ca.Value).ToString() == source));
        }
    }
}
