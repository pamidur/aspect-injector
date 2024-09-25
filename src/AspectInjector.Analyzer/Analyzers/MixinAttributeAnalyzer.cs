using AspectInjector.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MixinAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                EffectRules.MixinSupportsOnlyInterfaces.AsDescriptor()
                , EffectRules.EffectMustBePartOfAspect.AsDescriptor()
                , EffectRules.MixinSupportsOnlyAspectInterfaces.AsDescriptor()
                );

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.MixinType)
                return;

            var location = context.Node.GetLocation();

            if (!context.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType))
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.EffectMustBePartOfAspect.AsDescriptor(), location, context.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            if (attr.ConstructorArguments[0].Value is INamedTypeSymbol arg)
            {
                if (arg.TypeKind == TypeKind.Error)
                    return;

                if (arg.TypeKind != TypeKind.Interface)
                    context.ReportDiagnostic(Diagnostic.Create(EffectRules.MixinSupportsOnlyInterfaces.AsDescriptor(), location, arg.Name));
                else if (context.ContainingSymbol is INamedTypeSymbol aspectClass && !aspectClass.AllInterfaces.Any(i => i == arg))
                    context.ReportDiagnostic(Diagnostic.Create(EffectRules.MixinSupportsOnlyAspectInterfaces.AsDescriptor(), location, ImmutableDictionary<string, string>.Empty.Add(WellKnown.MixinTypeProperty, arg.Name), context.ContainingSymbol.Name, arg.Name));
            }
        }
    }
}
