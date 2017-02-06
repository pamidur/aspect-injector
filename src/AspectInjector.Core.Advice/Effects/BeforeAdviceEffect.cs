using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    public class BeforeAdviceEffect : AdviceEffectBase
    {
        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            return base.IsApplicableFor(target);
        }
    }
}