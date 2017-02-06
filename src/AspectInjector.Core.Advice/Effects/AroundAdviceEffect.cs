using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    public class AroundAdviceEffect : AdviceEffectBase
    {
        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }
    }
}