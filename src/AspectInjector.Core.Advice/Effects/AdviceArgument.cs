using AspectInjector.Broker;
using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AdviceArgument
    {
        public Source Source { get; set; }

        public ParameterDefinition Parameter { get; set; }
    }
}