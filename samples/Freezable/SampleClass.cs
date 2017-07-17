using AspectInjector.Broker;

namespace AspectInjector.SampleApps.Freezable
{
    [Inject(typeof(FreezableAspect))]
    class SampleClass
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
