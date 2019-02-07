using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AroundAdviceEffect : AdviceEffectBase
    {
        public override Kind Kind => Kind.Around;

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            if (target is MethodDefinition method && method.IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            var result = base.Validate(aspect, log);

            if (Method.ReturnType != Method.Module.TypeSystem.Object)
            {
                log.Log(EffectRules.AdviceMustHaveValidSingnature, Method, Method.Name, EffectRules.Literals.MustBeObjectForAround);
                result = false;
            }

            return result;
        }

        protected override void ValidateSupportedArguments(AspectDefinition aspectDefinition, ILogger log)
        {
            var wrongArgs = Arguments.Where(a => a.Source == Source.ReturnValue).ToArray();
            LogWrongArgs(wrongArgs, aspectDefinition, log);
        }
    }
}