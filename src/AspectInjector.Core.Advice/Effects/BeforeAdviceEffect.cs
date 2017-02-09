using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    internal class BeforeAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.Before;

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            //check args

            return base.IsApplicableFor(target);
        }
    }
}