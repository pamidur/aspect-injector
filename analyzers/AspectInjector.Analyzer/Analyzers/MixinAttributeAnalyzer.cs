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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rules.MixinSupportsOnlyInterfaces, Rules.MixinMustBePartOfAspect, Rules.MixinSupportsOnlyAspectInterfaces); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != WellKnown.MixinType.FullName)
                return;

            var location = context.Node.GetLocation();

            if (!context.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == WellKnown.AspectType.FullName))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MixinMustBePartOfAspect, location, context.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            var arg = (INamedTypeSymbol)attr.ConstructorArguments[0].Value;

            if (arg.TypeKind == TypeKind.Error)
                return;

            if (arg.TypeKind != TypeKind.Interface)
                context.ReportDiagnostic(Diagnostic.Create(Rules.MixinSupportsOnlyInterfaces, context.Node.GetLocation(), arg.Name));
            else if (context.ContainingSymbol is INamedTypeSymbol aspectClass && !aspectClass.AllInterfaces.Any(i => i == arg))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MixinSupportsOnlyAspectInterfaces, location,ImmutableDictionary<string, string>.Empty.Add(WellKnown.MixinTypeProperty, arg.Name), context.ContainingSymbol.Name, arg.Name));

        }
    }
}
