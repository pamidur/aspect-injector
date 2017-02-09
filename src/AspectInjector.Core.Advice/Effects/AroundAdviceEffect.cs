using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AroundAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.Around;

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            //check args

            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }
    }
}