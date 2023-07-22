using AspectInjector.Broker;
using AspectInjector.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AdviceAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                EffectRules.EffectMustBePartOfAspect.AsDescriptor()
                , EffectRules.AdviceMustHaveValidSingnature.AsDescriptor()
                , EffectRules.AdviceArgumentMustBeBound.AsDescriptor()
                , GeneralRules.UnknownCompilationOption.AsDescriptor()
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

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.AdviceType)
                return;

            if (!(context.ContainingSymbol is IMethodSymbol method))
                return;

            var location = context.Node.GetLocation();

            if (!method.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType))
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.EffectMustBePartOfAspect.AsDescriptor(), location, method.ContainingSymbol.Name));

            if (method.IsStatic)
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceMustHaveValidSingnature.AsDescriptor(), location, method.Name, EffectRules.Literals.IsStatic));

            if (method.IsGenericMethod)
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceMustHaveValidSingnature.AsDescriptor(), location, method.Name, EffectRules.Literals.IsGeneric));

            if (method.DeclaredAccessibility != Accessibility.Public)
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceMustHaveValidSingnature.AsDescriptor(), location, method.Name, EffectRules.Literals.IsNotPublic));

            foreach (var param in method.Parameters)
                if (!param.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceArgumentType))
                    context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceArgumentMustBeBound.AsDescriptor(), param.Locations.First(), param.Name));

            if (attr.AttributeConstructor == null)
                return;

            var kindArg = attr.ConstructorArguments[0].Value;
            if (kindArg != null)
            {
                var kind = (Kind)Enum.ToObject(typeof(Kind), kindArg);

                if (!Enum.IsDefined(typeof(Kind), kind))
                    context.ReportDiagnostic(Diagnostic.Create(GeneralRules.UnknownCompilationOption.AsDescriptor(), location, GeneralRules.Literals.UnknownAdviceKind(kind.ToString())));

                if (kind == Kind.Around)
                {
                    if (method.ReturnType.SpecialType != SpecialType.System_Object)
                        context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceMustHaveValidSingnature.AsDescriptor(), location, method.Name, EffectRules.Literals.MustBeObjectForAround));
                }
                else
                {
                    if (method.ReturnType.SpecialType != SpecialType.System_Void)
                        context.ReportDiagnostic(Diagnostic.Create(EffectRules.AdviceMustHaveValidSingnature.AsDescriptor(), location, method.Name, EffectRules.Literals.MustBeVoidForInline));
                }
            }

            var atarget = attr.NamedArguments.FirstOrDefault(n => n.Key == nameof(Advice.Targets)).Value.Value;
            if (atarget != null)
            {
                var t = (Target)Enum.ToObject(typeof(Target), atarget);
                if (t > Target.Any)
                    context.ReportDiagnostic(Diagnostic.Create(GeneralRules.UnknownCompilationOption.AsDescriptor(), location, GeneralRules.Literals.UnknownAdviceTarget(t.ToString())));
            }
        }
    }
}
