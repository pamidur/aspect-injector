using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAfterProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private VariableDefinition _retvar;

        public AdviceAfterProcess(ILogger log, MethodDefinition target, AspectDefinition aspect, AfterAdviceEffect effect)
            : base(log, target, effect, aspect)
        {
        }

        public override void Execute()
        {
            _target.GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }
    }
}