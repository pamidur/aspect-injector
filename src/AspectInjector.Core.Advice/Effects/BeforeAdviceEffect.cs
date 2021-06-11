using AspectInjector.Broker;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;

namespace AspectInjector.Core.Advice.Effects
{
    internal class BeforeAdviceEffect : AdviceEffectBase
    {
        public override Kind Kind => Kind.Before;

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            var result = base.Validate(aspect, log);

            if (!Method.ReturnType.Match(Method.Module.TypeSystem.Void))
            {
                log.Log(EffectRules.AdviceMustHaveValidSingnature, Method, Method.Name, EffectRules.Literals.MustBeVoidForInline);
                result = false;
            }

            return result;
        }
    }
}