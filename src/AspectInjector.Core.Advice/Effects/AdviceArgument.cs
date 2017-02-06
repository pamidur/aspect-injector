using static AspectInjector.Broker.Advice;

namespace AspectInjector.Core.Advice.Effects
{
    public class AdviceArgument
    {
        public Argument.Source Source { get; set; }

        public int Index { get; set; }
    }
}