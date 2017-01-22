using Mono.Cecil;

namespace AspectInjector.Core.Models
{
    public class Injection
    {
        public ICustomAttributeProvider Target { get; set; }

        public uint Priority { get; internal set; }

        public AspectDefinition Source { get; internal set; }

        public Effect Effect { get; internal set; }
    }
}