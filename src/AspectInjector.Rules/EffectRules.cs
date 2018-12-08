using AspectInjector.Broker;

namespace AspectInjector.Rules
{
    public static class EffectRules
    {        
        public static class Literals
        {
            public static readonly string IsStatic = "is static";
            public static readonly string IsGeneric = "is generic";
            public static readonly string IsNotPublic = "is not public";
            public static readonly string MustBeObjectForAround = "must return 'object' for 'Around' advice";
            public static readonly string MustBeVoidForInline = "must be 'void' for 'After' or 'Before' advice";


            public static readonly string ObjectArray = "object[]";
            public static readonly string AttributeArray = "System.Attribute[]";
            public static readonly string Object = "object";
            public static readonly string String = "string";
            public static readonly string Type = "System.Type";
            public static readonly string MethodBase = "System.Reflection.MethodBase";

            public static readonly string TargetFunc = "System.Func<object[],object>";

        }

        public static readonly Rule EffectMustBePartOfAspect =
            GeneralRules.Make("AI_E000",
                "Effect must be a part of an Aspect",
                "'{0}' is not an Aspect",
                "Unbound Effects are not supported.");


        public static readonly Rule MixinSupportsOnlyInterfaces =
            GeneralRules.Make("AI_EM00",
                "Mixin supports only interfaces",
                "'{0}' is not an interface",
                "Non-interface types are not supported in this context.");

        public static readonly Rule MixinSupportsOnlyAspectInterfaces =
            GeneralRules.Make("AI_EM01",
               "Mixin supports only implemented interfaces",
               "'{0}' does not implement '{1}'",
               "Aspect must implement mixin interface.");

        

        public static readonly Rule AdviceMustHaveValidSingnature =
            GeneralRules.Make("AI_EA00",
               "Advice must have invalid signature",
               "Advice '{0}' {1}",
               "Advice must be non-static, non-generic, public method. Return 'void' if inline and 'object' otherwise.");      

        public static readonly Rule AdviceArgumentMustBeBound =
            GeneralRules.Make("AI_EA01",
               "Advice Argument must be bound",
               "Argument '{0}' is unbound",
               "Unbound arguments are not supported.");


        public static readonly Rule ArgumentMustBePartOfAdvice =
            GeneralRules.Make("AI_EA02",
                "Advice Argument must be a part of an Advice",
                "Method '{0}' is not an Advice",
                "Unbound Advice Arguments are not supported.");

        public static readonly Rule ArgumentIsAlwaysNull =
            GeneralRules.Make("AI_EA03",
               "Argument source is always null in this context",
               "Argument '{0}' is always null for '{1}' advice",
               $"Argument source returns null in current context. '{nameof(Source.Target)}' should be used with {nameof(Kind.Around)} advice, and '{nameof(Source.ReturnValue)}' should be used with {nameof(Kind.After)} advice",
               RuleSeverity.Warning);

        public static readonly Rule ArgumentMustHaveValidType =
            GeneralRules.Make("AI_EA04",
                "Argument for source has invalid type",
                "'{0}' has invalid type, {1} expected",
                "Argument must have valid type.");
    }
}
