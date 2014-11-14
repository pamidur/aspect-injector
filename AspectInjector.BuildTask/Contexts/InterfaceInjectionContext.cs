using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Contexts
{
    public class InterfaceInjectionContext : IInjectionContext
    {
        public AspectInjectionInfo AspectContext { get; set; }

        public TypeDefinition InterfaceDefinition { get; set; }

        public EventDefinition[] Events { get; set; }

        public PropertyDefinition[] Properties { get; set; }

        public MethodDefinition[] Methods { get; set; }
    }
}