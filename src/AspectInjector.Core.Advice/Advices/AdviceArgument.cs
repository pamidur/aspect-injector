using static AspectInjector.Broker.Advice;

namespace AspectInjector.Core.Advice.Advices
{
    public class AdviceArgument
    {
        public Argument Argument { get; set; }

        public int Index { get; set; }
    }
}