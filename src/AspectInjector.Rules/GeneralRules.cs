using FluentIL.Common;
using System;

namespace AspectInjector.Rules
{
    public static class GeneralRules
    {
        internal static Rule Make(string id, string title, string message, string description, RuleSeverity severity = RuleSeverity.Error)
        => new Rule(id, title, message, severity, description: description, helpLinkUri: $"https://github.com/pamidur/aspect-injector/blob/master/docs/errors/{id}.md");

        public static class Literals
        {
            public static readonly Func<string, string> UnknownAdviceKind = k => $"Unknown advice kind '{k}'";
            public static readonly Func<string, string> UnknownAdviceTarget = t => $"Unknown advice target '{t}'";
            public static readonly Func<string, string> UnknownPropagationStrategy = t => $"Unknown propagation strategy '{t}'";
            public static readonly Func<string, string> InvalidPropagationFilter = t => $"Invalid propagation filter '{t}'";
            public static readonly Func<string, string> UnknownArgumentSource = s => $"Unknown argument source '{s}'";
            public static readonly Func<string, string> UnknownAspectScope = s => $"Unknown aspect scope '{s}'";
        }

        public static readonly Rule Info = 
            Make("AI_INFO",
                "Info message",
                "{0}",
                "Thanks for using https://github.com/pamidur/aspect-injector",
                RuleSeverity.Info);


        public static readonly Rule CompilationMustSecceedIfNoOtherErrors =
            Make("AI_ERR0",
                "Aspect Injector encounters error it wasn't expected",
                "{0}. Please submit an issue to https://github.com/pamidur/aspect-injector",
                "Aspect Injector encounters error it wasn't expected. Please submit an issue to https://github.com/pamidur/aspect-injector");

        public static readonly Rule UnknownCompilationOption =
            Make("AI_ERR1",
                "Aspect Injector encounters unknown compilation option",
                "{0}. Please make sure your AspectInjector version is up to date.",
                "Aspect Injector encounters error it wasn't expected. If AspectInjector is up to date - please submit an issue to https://github.com/pamidur/aspect-injector");

        public static readonly Rule UnexpectedCompilerBehaviour =
            Make("AI_ERR2",
                "Aspect Injector encounters unexpected behaviour",
                "{0}. Please submit an issue to https://github.com/pamidur/aspect-injector",
                "Aspect Injector encounters behavior it wasn't expected. Please submit an issue to https://github.com/pamidur/aspect-injector");
    }
}
