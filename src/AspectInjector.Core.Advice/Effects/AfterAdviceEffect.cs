namespace AspectInjector.Core.Advice.Effects
{
    internal class AfterAdviceEffect : BeforeAdviceEffect
    {
        public override Broker.Advice.Kind Kind => Broker.Advice.Kind.After;
    }
}