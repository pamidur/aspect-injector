using AspectInjector.Broker;
using Microsoft.CodeAnalysis;

namespace AspectInjector.Analyzer
{
    public static class Rules
    {
        private static DiagnosticDescriptor Make(string id, string title, string message, string description, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => new DiagnosticDescriptor(id, title, message, "Aspects", severity, isEnabledByDefault: true, description: description, helpLinkUri: $"https://pamidur.github.io/aspect-injector/errors/{id}");


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
               "Aspect '{0}' does not have any effect",
               "Aspect should have at least one effect - Advice, Mixin etc.",
                DiagnosticSeverity.Warning);

        public static readonly DiagnosticDescriptor AspectMustNotBeGeneric =
            Make("AIAM005",
               "Aspect must not be generic",
               "Aspect '{0}' has generic parameters",
               "Generic Aspects are not supported.");

        public static readonly DiagnosticDescriptor AspectMustNotBeAbstract =
            Make("AIAM006",
               "Aspect must not be abstract",
               "Aspect '{0}' is abstract",
               "Abstract Aspect classes are not supported.");

        public static readonly DiagnosticDescriptor AspectMustNotBeStatic =
            Make("AIAM007",
               "Aspect must not be static",
               "Aspect '{0}' is static",
               "Static Aspect classes are not supported.");

        public static readonly DiagnosticDescriptor AspectFactoryMustContainFactoryMethod =
            Make("AIAM008",
               "Aspect factory must contain factory method",
               "Factory type '{0}' does not contain factory method",
               "Aspect factory must contain method 'public static object GetInstance(Type)'.");

        public static readonly DiagnosticDescriptor AspectMustHaveContructorOrFactory =
            Make("AIAM009",
               "Aspect must have public parameterless constructor or factory",
               "Aspect '{0}' does not have public parameterless constructor nor defined factory",
               "Aspect must have public parameterless constructor or defined factory.");




        public static readonly DiagnosticDescriptor AdviceMustBePartOfAspect =
            Make("AIAM010",
                "Advice must be a part of an Aspect",
                "'{0}' is not an Aspect",
                "Unbound Advices are not supported.");

        public static readonly DiagnosticDescriptor AdviceMustNotBeStatic =
            Make("AIAM011",
               "Advice must not be static",
               "Advice '{0}' is static",
               "Static Advice methods are not supported.");

        public static readonly DiagnosticDescriptor AdviceMustNotBeGeneric =
            Make("AIAM019",
               "Advice must not be generic",
               "Advice '{0}' is generic",
               "Generic Advice methods are not supported.");

        public static readonly DiagnosticDescriptor AdviceMustBePublic =
            Make("AIAM012",
               "Advice must be public",
               "Advice '{0}' is not public",
               "Non-public Advice methods are not supported.");

        public static readonly DiagnosticDescriptor AdviceAroundMustReturnObject =
            Make("AIAM013",
               "Around Advice must return object",
               "Advice '{0}' does not have return value",
               "Around Advice must return object that will casted to real method result.");

        public static readonly DiagnosticDescriptor AdviceInlineMustBeVoid =
            Make("AIAM014",
               "Inline Advice must be void",
               "Advice '{0}' returns value",
               "Inline Advice must be void as returned value is not supported.");

        public static readonly DiagnosticDescriptor AdviceArgumentMustBeBound =
            Make("AIAM016",
               "Advice Argument must be bound",
               "Argument '{0}' is unbound",
               "Unbound arguments are not supported.");


        public static readonly DiagnosticDescriptor ArgumentMustBePartOfAdvice =
            Make("AIAM015",
                "Advice Argument must be a part of an Advice",
                "'{0}' is not an Advice",
                "Unbound Advice Arguments are not supported.");

        public static readonly DiagnosticDescriptor ArgumentTargetShouldBeUsedForAroundAdvice =
            Make("AIAM017",
               $"Argument source '{nameof(Advice.Argument.Source.Target)}' is valid with Aro{nameof(Advice.Type.Around)}und advice only",
               "Argument '{0}' has invalid source",
               $"Argument source '{nameof(Advice.Argument.Source.Target)}' should be used with {nameof(Advice.Type.Around)} advice only.",
               DiagnosticSeverity.Warning);

        public static readonly DiagnosticDescriptor ArgumentReturnValueShouldBeUsedForAfterAdvice =
            Make("AIAM018",
               $"Argument source '{nameof(Advice.Argument.Source.ReturnValue)}' is valid with {nameof(Advice.Type.After)} After advice only",
               "Argument '{0}' has invalid source",
               $"Argument source '{nameof(Advice.Argument.Source.ReturnValue)}' should be used with {nameof(Advice.Type.After)} advice only.",
               DiagnosticSeverity.Warning);

    }
}
