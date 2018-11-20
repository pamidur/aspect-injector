using Microsoft.CodeAnalysis;

namespace AspectInjector.Analyzer
{
    public static class Rules
    {
        public static readonly DiagnosticDescriptor MixinSupportsOnlyInterfaces =
            Make("AIAM001",
                "Mixin supports only interfaces",
                "'{0}' is not an interface",
                "Non-interface types are not supported in this context.");

        public static readonly DiagnosticDescriptor MixinMustBePartOfAspect =
            Make("AIAM002",
                "Mixin must be a part of an Aspect",
                "'{0}' is not an Aspect",
                "Unbound Mixins are not supported.");

        public static readonly DiagnosticDescriptor MixinSupportsOnlyAspectInterfaces =
            Make("AIAM003",
               "Mixin supports only implemented interfaces",
               "'{0}' does not implement '{1}'",
               "Aspect must implement mixin interface.");



        public static readonly DiagnosticDescriptor AspectShouldContainEffect =
            Make("AIAM004",
               "Aspect should contain effect",
               "'{0}' does not have any effect",
               "Aspect should have at least one effect - Advice, Mixin etc.",
                DiagnosticSeverity.Warning);

        public static readonly DiagnosticDescriptor AspectMustNotBeGeneric =
            Make("AIAM005",
               "Aspect must not be generic",
               "'{0}' has generic parameters",
               "Generic Aspects are not supported.");

        public static readonly DiagnosticDescriptor AspectMustNotBeAbstract =
            Make("AIAM006",
               "Aspect must not be abstract",
               "'{0}' is abstract",
               "Abstract Aspect classes are not supported.");

        public static readonly DiagnosticDescriptor AspectMustNotBeStatic =
            Make("AIAM007",
               "Aspect must not be static",
               "'{0}' is static",
               "Static Aspect classes are not supported.");

        public static readonly DiagnosticDescriptor AspectFactoryMustContainFactoryMethod =
            Make("AIAM008",
               "Aspect factory must contain factory method",
               "Factory type '{0}' does not contain factory method",
               "Aspect factory must contain method 'public static object GetInstance(Type)'.");

        public static readonly DiagnosticDescriptor AspectMustHaveContructorOrFactory =
            Make("AIAM009",
               "Aspect must have public parameterless constructor or factory",
               "'{0}' does not have public parameterless constructor nor defined factory",
               "Aspect must have public parameterless constructor or defined factory.");


        private static DiagnosticDescriptor Make(string id, string title, string message, string description, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => new DiagnosticDescriptor(id, title, message, "Aspects", severity, isEnabledByDefault: true, description: description, helpLinkUri: $"https://pamidur.github.io/aspect-injector/errors/{id}");
    }
}
