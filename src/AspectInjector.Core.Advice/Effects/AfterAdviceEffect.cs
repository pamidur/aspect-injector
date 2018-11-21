namespace AspectInjector.Core.Advice.Effects
{
    internal class AfterAdviceEffect : BeforeAdviceEffect
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.After;
    }
}