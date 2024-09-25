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
    public class ArgumentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                EffectRules.ArgumentMustBePartOfAdvice.AsDescriptor()
                , EffectRules.ArgumentIsAlwaysNull.AsDescriptor()
                , EffectRules.ArgumentMustHaveValidType.AsDescriptor()
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
            if (!(context.SemanticModel.GetDeclaredSymbol(context.Node.Parent.Parent) is IParameterSymbol param))
                return;

            var attr = param.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.AdviceArgumentType)
                return;

            var location = context.Node.GetLocation();

            var adviceattr = param.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceType);

            if (adviceattr == null)
                context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustBePartOfAdvice.AsDescriptor(), location, param.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            var sourceArg = attr.ConstructorArguments[0].Value;
            if (sourceArg != null)
            {
                var source = (Source)Enum.ToObject(typeof(Source), sourceArg);

                switch (source)
                {
                    case Source.Arguments:
                        if (param.Type.ToDisplayString() != "object[]")
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.ObjectArray));
                        break;
                    case Source.Instance:
                        if (param.Type.SpecialType != SpecialType.System_Object)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.Object));
                        break;
                    case Source.Metadata:
                        if (param.Type.ToDisplayString() != WellKnown.MethodBase)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.MethodBase));
                        break;
                    case Source.Name:
                        if (param.Type.SpecialType != SpecialType.System_String)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.String));
                        break;
                    case Source.ReturnType:
                        if (param.Type.ToDisplayString() != WellKnown.Type)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.Type));
                        break;
                    case Source.ReturnValue:
                        if (param.Type.SpecialType != SpecialType.System_Object)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.Object));
                        break;
                    case Source.Target:
                        if (param.Type.ToDisplayString() != "System.Func<object[], object>")
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.TargetFunc));
                        break;
                    case Source.Type:
                        if (param.Type.ToDisplayString() != WellKnown.Type)
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.Type));
                        break;
                    case Source.Triggers:
                        if (param.Type.ToDisplayString() != "System.Attribute[]")
                            context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentMustHaveValidType.AsDescriptor(), location, param.Name, EffectRules.Literals.AttributeArray));
                        break;
                    default:
                        context.ReportDiagnostic(Diagnostic.Create(GeneralRules.UnknownCompilationOption.AsDescriptor(), location, GeneralRules.Literals.UnknownArgumentSource(source.ToString())));
                        break;
                }

                if (adviceattr == null || adviceattr.AttributeConstructor == null)
                    return;

                var kindArg = adviceattr.ConstructorArguments[0].Value;

                if(kindArg!=null)
                {
                    var kind = (Kind)Enum.ToObject(typeof(Kind), kindArg);
 
                    if (source == Source.Target && kind != Kind.Around)
                        context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentIsAlwaysNull.AsDescriptor(), location, param.Name, kind));

                    if (source == Source.ReturnValue && kind != Kind.After)
                        context.ReportDiagnostic(Diagnostic.Create(EffectRules.ArgumentIsAlwaysNull.AsDescriptor(), location, param.Name, kind));
                }
            }
        }
    }
}
