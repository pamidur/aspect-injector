using Mono.Cecil;

namespace AspectInjector.Core.Contexts
{
    public class ProcessingContext
    {
        public AssemblyDefinition Assembly { get; internal set; }

        public IAssemblyResolver Resolver { get; set; }

        public ServicesContext Services { get; set; }
    }
}