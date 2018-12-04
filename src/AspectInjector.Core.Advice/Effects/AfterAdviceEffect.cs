using AspectInjector.Broker;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AfterAdviceEffect : BeforeAdviceEffect
    {
        public override Kind Kind => Kind.After;
    }
}