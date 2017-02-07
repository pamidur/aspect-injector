using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    public class BeforeAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.Before;

        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            return base.IsApplicableFor(target);
        }
    }
}