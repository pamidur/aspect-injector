using AspectInjector.Broker;
using Microsoft.CodeAnalysis;

namespace AspectInjector.Analyzer
{
    public static class Rules
    {
        private static DiagnosticDescriptor Make(string id, string title, string message, string description, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => new DiagnosticDescriptor(id, title, message, "Aspects", severity, isEnabledByDefault: true, description: description, helpLinkUri: $"https://pamidur.github.io/aspect-injector/errors/{id}");


        public static readonly DiagnosticDescriptor EffectMustBePartOfAspect =
            Make("AIAM001",
                "Effect must be a part of an Aspect",
                "'{0}' is not an Aspect",
                "Unbound Effects are not supported.");



        public static readonly DiagnosticDescriptor MixinSupportsOnlyInterfaces =
            Make("AIAM002",
                "Mixin supports only interfaces",
                "'{0}' is not an interface",
                "Non-interface types are not supported in this context.");

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

        public static readonly DiagnosticDescriptor AspectMustHaveValidSignature =
            Make("AIAM005",
               "Aspect must not be generic, abstract or static",
               "Aspect '{0}' {1}",
               "Aspect must have valid signature. Aspect must be non-generic, non-abstract and non-static class.");
        
        public static readonly DiagnosticDescriptor AspectFactoryMustContainFactoryMethod =
            Make("AIAM006",
               "Aspect factory must contain factory method",
               "Factory type '{0}' does not contain factory method",
               "Aspect factory must contain method 'public static object GetInstance(Type)'.");

        public static readonly DiagnosticDescriptor AspectMustHaveContructorOrFactory =
            Make("AIAM007",
               "Aspect must have public parameterless constructor or factory",
               "Aspect '{0}' does not have public parameterless constructor nor defined factory",
               "Aspect must have public parameterless constructor or defined factory.");

        

        public static readonly DiagnosticDescriptor AdviceMustHaveValidSingnature =
            Make("AIAM008",
               "Advice must have invalid signature",
               "Advice '{0}' {1}",
               "Advice must be non-static, non-generic, public method. Return 'void' if inline and 'object' otherwise.");      

        public static readonly DiagnosticDescriptor AdviceArgumentMustBeBound =
            Make("AIAM009",
               "Advice Argument must be bound",
               "Argument '{0}' is unbound",
               "Unbound arguments are not supported.");



        public static readonly DiagnosticDescriptor ArgumentMustBePartOfAdvice =
            Make("AIAM010",
                "Advice Argument must be a part of an Advice",
                "Method '{0}' is not an Advice",
                "Unbound Advice Arguments are not supported.");

        public static readonly DiagnosticDescriptor ArgumentIsAlwaysNull =
            Make("AIAM011",
               "Argument source is always null in this context",
               "Argument '{0}' is always null {1}",
               $"Argument source returns null in current context. '{nameof(Source.Target)}' should be used with {nameof(Kind.Around)} advice, and '{nameof(Source.ReturnValue)}' should be used with {nameof(Kind.After)} advice",
               DiagnosticSeverity.Warning);

        public static readonly DiagnosticDescriptor ArgumentMustHaveValidType =
            Make("AIAM012",
                "Argument for source has invalid type",
                "'{0}' has invalid type, {1} expected",
                "Argument must have valid type.");
    }
}
