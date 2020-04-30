using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Advice.Weavers.Processes;
using AspectInjector.Core.Models;
using FluentIL.Logging;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceAroundWeaver : AdviceInlineWeaver
    {
        public override byte Priority => 30;

        public AdviceAroundWeaver(ILogger logger) : base(logger)
        {
        }

        public override bool CanWeave(InjectionDefinition injection)
        {
            return injection.Effect is AroundAdviceEffect &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);
        }

        protected override void WeaveMethod(MethodDefinition method, InjectionDefinition injection)
        {
            if (injection.Effect is AroundAdviceEffect)
            {
                var process = new AdviceAroundProcess(_log, method, injection);
                process.Execute();
            }
            else
            {
                throw new NotSupportedException($"Unknown effect type. {injection.Effect?.GetType().Name}");
            }
        }
    }
}