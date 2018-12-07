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
    public class AspectAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                AspectRules.AspectMustHaveValidSignature
                , AspectRules.AspectMustHaveContructorOrFactory
                , AspectRules.AspectFactoryMustContainFactoryMethod
                , AspectRules.AspectShouldContainEffect
                , GeneralRules.UnknownCompilationOption
                );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.AspectType)
                return;

            var symbol = context.ContainingSymbol as INamedTypeSymbol;
            if (symbol == null)
                return;

            var factory = attr.NamedArguments.FirstOrDefault(n => n.Key == nameof(Aspect.Factory)).Value.Value;
            var ctor = symbol.Constructors.FirstOrDefault(m => m.Parameters.IsEmpty);

            var location = context.Node.GetLocation();

            if (attr.AttributeConstructor != null)
            {
                var scopeArg = attr.ConstructorArguments[0].Value;
                if (scopeArg != null)
                {
                    var scope = (Scope)Enum.ToObject(typeof(Scope), scopeArg);

                    if (!Enum.IsDefined(typeof(Scope), scope))
                        context.ReportDiagnostic(Diagnostic.Create(GeneralRules.UnknownCompilationOption, location, GeneralRules.Literals.UnknownAspectScope(scope.ToString())));
                }
            }


            if (symbol.IsStatic)
                context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectMustHaveValidSignature, location, symbol.Name, AspectRules.Literals.IsStatic));

            if (symbol.IsAbstract)
                context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectMustHaveValidSignature, location, symbol.Name, AspectRules.Literals.IsAbstract));

            if (symbol.IsGenericType)
                context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectMustHaveValidSignature, location, symbol.Name, AspectRules.Literals.HasGenericParams));

            if (!symbol.IsStatic && factory == null && ctor == null)
                context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectMustHaveContructorOrFactory, location, symbol.Name));

            if (factory is INamedTypeSymbol named)
            {
                var method = named.GetMembers("GetInstance").OfType<IMethodSymbol>().FirstOrDefault();

                if (method == null
                    || !method.IsStatic
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.ReturnType.SpecialType != SpecialType.System_Object
                    || method.Parameters.Length != 1
                    || method.Parameters[0].Type.ToDisplayString() != WellKnown.Type)
                {
                    context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectFactoryMustContainFactoryMethod, location, symbol.Name));
                }
            }

            var hasAdvices = symbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.Kind == SymbolKind.Method)
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AdviceType)).Any();

            var hasMixins = symbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.MixinType);

            if (!hasMixins && !hasAdvices)
                context.ReportDiagnostic(Diagnostic.Create(AspectRules.AspectShouldContainEffect, location, symbol.Name));
        }
    }
}
