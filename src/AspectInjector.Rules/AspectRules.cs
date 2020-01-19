using FluentIL.Common;

namespace AspectInjector.Rules
{
    public static class AspectRules
    {
        public static class Literals
        {
            public static readonly string HasGenericParams = "has generic parameters";
            public static readonly string IsAbstract = "is abstract";
            public static readonly string IsNotPublic = "is not public";
            public static readonly string IsStatic = "is static";
        }

        public static readonly Rule AspectShouldContainEffect =
            GeneralRules.Make("AI_A000",
               "Aspect should contain effect",
               "Aspect '{0}' does not have any effect",
               "Aspect should have at least one effect - Advice, Mixin etc.",
                RuleSeverity.Warning);

        public static readonly Rule AspectMustHaveValidSignature =
            GeneralRules.Make("AI_A001",
               "Aspect must not be generic, abstract or static. And it should be public.",
               "Aspect '{0}' {1}",
               "Aspect must have valid signature. Aspect must be non-generic, non-abstract and non-static public class.");

        public static readonly Rule AspectFactoryMustContainFactoryMethod =
            GeneralRules.Make("AI_A002",
               "Aspect factory must contain factory method",
               "Factory type '{0}' does not contain factory method",
               "Aspect factory must contain method 'public static object GetInstance(Type)'.");

        public static readonly Rule AspectMustHaveContructorOrFactory =
            GeneralRules.Make("AI_A003",
               "Aspect must have public parameterless constructor or factory",
               "Aspect '{0}' does not have public parameterless constructor nor defined factory",
               "Aspect must have public parameterless constructor or defined factory.");

    }
}
