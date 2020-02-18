using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Logging;
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
            _method.Body.OnAspectsInitialized(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }
    }
}