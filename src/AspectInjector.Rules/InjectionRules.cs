using Microsoft.CodeAnalysis;

namespace AspectInjector.Rules
{
    public static class InjectionRules
    {
        public static readonly DiagnosticDescriptor InjectionMustReferToAspect =
            GeneralRules.Make("AI_I000",
                "Injection must refer to Aspect type",
                "'{0}' is not an Aspect",
                "Non-aspect types cannot be injected.");

        public static readonly DiagnosticDescriptor InjectionMustBeAttribute =
            GeneralRules.Make("AI_I001",
                "Injection trigger must be an Attribute class",
                "'{0}' is not an Attribute",
                "Injection Attribute can only mark an attribute class.");
    }
}
