using AspectInjector.Broker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AdviceAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                Rules.EffectMustBePartOfAspect
                , Rules.AdviceMustHaveValidSingnature
                , Rules.AdviceArgumentMustBeBound
                );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.AdviceType)
                return;

            var method = context.ContainingSymbol as IMethodSymbol;
            if (method == null)
                return;

            var location = context.Node.GetLocation();

            if (!method.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType))
                context.ReportDiagnostic(Diagnostic.Create(Rules.EffectMustBePartOfAspect, location, method.ContainingSymbol.Name));

            if (method.IsStatic)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceMustHaveValidSingnature, location, method.Name, "is static"));

            if (method.IsGenericMethod)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceMustHaveValidSingnature, location, method.Name, "is generic"));

            if (method.DeclaredAccessibility != Accessibility.Public)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceMustHaveValidSingnature, location, method.Name, "is not public"));

            foreach (var param in method.Parameters)
                if (!param.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceArgumentType))
                    context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceArgumentMustBeBound, param.Locations.First(), param.Name));

            if (attr.AttributeConstructor == null)
                return;

            var atype = (Advice.Type)attr.ConstructorArguments[0].Value;

            if (atype == Advice.Type.Around)
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Object)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceMustHaveValidSingnature, location, method.Name, "does not return 'object' for Around"));
            }
            else
            {
                if (method.ReturnType.SpecialType != SpecialType.System_Void)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.AdviceMustHaveValidSingnature, location, method.Name, "does not return 'void' for After or Before"));
            }
        }
    }
}
