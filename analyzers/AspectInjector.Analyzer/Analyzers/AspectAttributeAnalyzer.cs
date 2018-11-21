using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AspectAttributeAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly string FactoryTypeProperty = "factory_type";
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                Rules.AspectMustNotBeStatic
                , Rules.AspectMustNotBeAbstract
                , Rules.AspectMustNotBeGeneric
                , Rules.AspectMustHaveContructorOrFactory
                , Rules.AspectFactoryMustContainFactoryMethod
                , Rules.AspectShouldContainEffect
                );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.AspectType.FullName)
                return;

            var symbol = context.ContainingSymbol as INamedTypeSymbol;
            if (symbol == null)
                return;

            var factory = attr.NamedArguments.FirstOrDefault(n => n.Key == nameof(Broker.Aspect.Factory)).Value.Value;
            var ctor = symbol.Constructors.FirstOrDefault(m => m.Parameters.IsEmpty);

            var location = context.Node.GetLocation();

            if (symbol.IsStatic)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectMustNotBeStatic, location, symbol.Name));

            if (symbol.IsAbstract)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectMustNotBeAbstract, location, symbol.Name));

            if (symbol.IsGenericType)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectMustNotBeGeneric, location, symbol.Name));

            if (!symbol.IsStatic && factory == null && ctor == null)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectMustHaveContructorOrFactory, location, symbol.Name));

            if (factory is INamedTypeSymbol named)
            {
                var method = named.GetMembers("GetInstance").OfType<IMethodSymbol>().FirstOrDefault();

                if (method == null
                    || !method.IsStatic
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.ReturnType.SpecialType != SpecialType.System_Object
                    || method.Parameters.Length != 1
                    || method.Parameters[0].Type.ToDisplayString() != WellKnown.Type.FullName)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.AspectFactoryMustContainFactoryMethod, location, symbol.Name));
                }
            }

            var advices = symbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.Kind == SymbolKind.Method)
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceType.FullName)).Any();

            var mixins = symbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.MixinType.FullName);

            if(!mixins && !advices)
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectShouldContainEffect, location, symbol.Name));
        }
    }
}
