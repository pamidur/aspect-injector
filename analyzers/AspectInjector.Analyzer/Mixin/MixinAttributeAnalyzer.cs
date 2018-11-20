using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AspectInjector.Analyzer.Mixin
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MixinAttributeAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly string MinixTypeProperty = "mixin_type";

        private static readonly Type _mixinType = typeof(Broker.Mixin);
        private static readonly Type _aspectType = typeof(Broker.Aspect);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rules.CanMixinOnlyInterfaces, Rules.MixinShouldBePartOfAspect, Rules.AspectShouldImplementMixin); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr == null || attr.AttributeClass.ToDisplayString() != _mixinType.FullName)
                return;

            var location = context.Node.GetLocation();

            if (!context.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == _aspectType.FullName))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MixinShouldBePartOfAspect, location, context.ContainingSymbol.Name));

            if (attr.AttributeConstructor == null)
                return;

            var arg = (INamedTypeSymbol)attr.ConstructorArguments[0].Value;

            if (arg.TypeKind == TypeKind.Error)
                return;

            if (arg.TypeKind != TypeKind.Interface)
                context.ReportDiagnostic(Diagnostic.Create(Rules.CanMixinOnlyInterfaces, context.Node.GetLocation(), arg.Name));
            else if (context.ContainingSymbol is INamedTypeSymbol aspectClass && !aspectClass.AllInterfaces.Any(i => i == arg))
                context.ReportDiagnostic(Diagnostic.Create(Rules.AspectShouldImplementMixin, location,ImmutableDictionary<string, string>.Empty.Add(MinixTypeProperty,arg.Name), context.ContainingSymbol.Name, arg.Name));

        }
    }
}
