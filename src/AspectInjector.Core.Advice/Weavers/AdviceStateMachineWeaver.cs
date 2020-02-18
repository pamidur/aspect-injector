using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Advice.Weavers.Processes;
using AspectInjector.Core.Models;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceStateMachineWeaver : AdviceInlineWeaver
    {
        public override byte Priority => 30;

        public AdviceStateMachineWeaver(ILogger logger) : base(logger)
        {
        }

        public override bool CanWeave(InjectionDefinition injection)
        {
            var target = injection.Target as MethodDefinition;
            return injection.Effect is AfterAdviceEffect && target != null && (target.IsAsync() || target.IsIterator());
        }

        protected override void WeaveMethod(MethodDefinition method, InjectionDefinition injection)
        {
            if (method.IsAsync())
                new AfterAsyncWeaveProcess(_log, method, injection).Execute();

            if (method.IsIterator())
                new AfterIteratorWeaveProcess(_log, method, injection).Execute();
        }
    }
}