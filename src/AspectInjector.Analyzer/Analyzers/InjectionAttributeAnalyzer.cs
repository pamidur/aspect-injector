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
    public class InjectionAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                InjectionRules.InjectionMustReferToAspect.AsDescriptor()
                , InjectionRules.InjectionMustBeAttribute.AsDescriptor()
                );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.InjectionType)
                return;

            var location = context.Node.GetLocation();

            var compilation = context.SemanticModel.Compilation;

            var attributeType = compilation.GetTypeByMetadataName(typeof(Attribute).FullName);

            if (context.ContainingSymbol is ITypeSymbol type && !compilation.ClassifyConversion(type, attributeType).IsImplicit)
                context.ReportDiagnostic(Diagnostic.Create(InjectionRules.InjectionMustBeAttribute.AsDescriptor(), location, context.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            if (attr.ConstructorArguments[0].Value is INamedTypeSymbol arg)
            {
                if (arg.TypeKind == TypeKind.Error)
                    return;

                if (!arg.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType))
                    context.ReportDiagnostic(Diagnostic.Create(InjectionRules.InjectionMustReferToAspect.AsDescriptor(), location, arg.Name));
            }
        }
    }
}
