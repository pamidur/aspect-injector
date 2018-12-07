using AspectInjector.Broker;

namespace AspectInjector.SampleApps.Freezable
{
    [Freezable]
    class SampleClass
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
