using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Advice.Weavers.Processes;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceStateMachineWeaver : AdviceInlineWeaver
    {
        public override byte Priority => 30;

        public AdviceStateMachineWeaver(ILogger logger) : base(logger)
        {
        }

        public override bool CanWeave(Injection injection)
        {
            var target = injection.Target as MethodDefinition;
            return injection.Effect is AfterAdviceEffect && target != null && (target.IsAsync() || target.IsIterator());
        }

        protected override void WeaveMethod(MethodDefinition method, Injection injection)
        {
            if (method.IsAsync())
                new AfterAsyncWeaveProcess(_log, method, (AfterAdviceEffect)injection.Effect, injection.Source).Execute();
        }
    }
}