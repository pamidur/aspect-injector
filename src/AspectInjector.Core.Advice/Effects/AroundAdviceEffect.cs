using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    public class AroundAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.Around;

        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }
    }
}