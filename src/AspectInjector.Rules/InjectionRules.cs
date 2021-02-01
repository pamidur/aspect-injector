using FluentIL.Common;

namespace AspectInjector.Rules
{
    public static class InjectionRules
    {
        public static readonly Rule InjectionMustReferToAspect =
            GeneralRules.Make("AI_I000",
                "Injection must refer to Aspect type",
                "'{0}' is not an Aspect",
                "Non-aspect types cannot be injected.");

        public static readonly Rule InjectionMustBeAttribute =
            GeneralRules.Make("AI_I001",
                "Injection trigger must be an Attribute class or Interface",
                "'{0}' is not an Attribute or interface",
                "Injection Attribute can only mark an attribute class or interface.");
    }
}
