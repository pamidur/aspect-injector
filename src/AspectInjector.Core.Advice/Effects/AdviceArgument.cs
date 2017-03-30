using Mono.Cecil;
using static AspectInjector.Broker.Advice;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AdviceArgument
    {
        public Argument.Source Source { get; set; }

        public ParameterDefinition Parameter { get; set; }
    }
}