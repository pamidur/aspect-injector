using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceBeforeProcess : AdviceWeaveProcessBase<BeforeAdviceEffect>
    {
        public AdviceBeforeProcess(ILogger log, MethodDefinition target, InjectionDefinition injection)
            : base(log, target, injection)
        {
        }

        public override void Execute()
        {
            _target.GetEditor().OnEntry(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }
    }
}