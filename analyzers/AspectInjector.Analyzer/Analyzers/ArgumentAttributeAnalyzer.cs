using AspectInjector.Broker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArgumentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                Rules.ArgumentMustBePartOfAdvice
                , Rules.ArgumentIsAlwaysNull
                , Rules.ArgumentMustHaveValidType
                );

        public override void Initialize(AnalysisContext context)
        {
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
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustBePartOfAdvice, location, param.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            var source = (Source)attr.ConstructorArguments[0].Value;


            if (source == Source.Arguments && param.Type.ToDisplayString() != "object[]")
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, $"object[]"));

            if (source == Source.Instance && param.Type.SpecialType != SpecialType.System_Object)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, $"object"));

            if (source == Source.Method && param.Type.ToDisplayString() != WellKnown.MethodBase)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, WellKnown.MethodBase));

            if (source == Source.Name && param.Type.SpecialType != SpecialType.System_String)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, "string"));

            if (source == Source.ReturnType && param.Type.ToDisplayString() != WellKnown.Type)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, WellKnown.Type));

            if (source == Source.ReturnValue && param.Type.SpecialType != SpecialType.System_Object)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, "object"));

            if (source == Source.Target && param.Type.ToDisplayString() != "System.Func<object[], object>")
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, "System.Func<object[],object>"));

            if (source == Source.Type && param.Type.ToDisplayString() != WellKnown.Type)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, WellKnown.Type));

            if (source == Source.Injections && param.Type.ToDisplayString() != "System.Attribute[]")
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentMustHaveValidType, location, param.Name, "System.Attribute[]"));


            if (adviceattr == null || adviceattr.AttributeConstructor == null)
                return;

            var adviceType = (Kind)adviceattr.ConstructorArguments[0].Value;

            if (source == Source.Target && adviceType != Kind.Around)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentIsAlwaysNull, location, param.Name, $"for '{adviceType}' advice"));

            if (source == Source.ReturnValue && adviceType != Kind.After)
                context.ReportDiagnostic(Diagnostic.Create(Rules.ArgumentIsAlwaysNull, location, param.Name, $"for '{adviceType}' advice"));
         }
    }
}
