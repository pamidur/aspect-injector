using Microsoft.CodeAnalysis;

namespace AspectInjector.Analyzer.Mixin
{
    public static class Rules
    {
        private static string Category = "Aspects";

        public static readonly DiagnosticDescriptor CanMixinOnlyInterfaces =
            new DiagnosticDescriptor("AIAM001", "Only interfaces are mixable", "'{0}' is not an Interface", Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Only interfaces can be mixed in. Non-interface types are not supported in this context.");

        public static readonly DiagnosticDescriptor MixinShouldBePartOfAspect =
            new DiagnosticDescriptor("AIAM002", "Mixin should be part of an Aspect", "'{0}' is not an Aspect", Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Mixin should be part of Aspect. Unbound Mixins are not supported.");

        public static readonly DiagnosticDescriptor AspectShouldImplementMixin =
           new DiagnosticDescriptor("AIAM003", "Aspect should implement mixed Interface", "'{0}' does not implement '{1}'", Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Aspect should implement mixed Interface.");



    }
}
