using System;
using AspectInjector.Broker;
using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AfterAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.After;

        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            // check args

            return base.IsApplicableFor(target);
        }
    }
}