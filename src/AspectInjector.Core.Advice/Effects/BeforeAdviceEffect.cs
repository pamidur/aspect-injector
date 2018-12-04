using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;

namespace AspectInjector.Core.Advice.Effects
{
    internal class BeforeAdviceEffect : AdviceEffectBase
    {
        public override Kind Kind => Kind.Before;

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (Method.ReturnType != Method.Module.TypeSystem.Void)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} should be void.", aspect.Host));
                return false;
            }

            return base.Validate(aspect, log);
        }
    }
}