using AspectInjector.Contracts;
using Mono.Cecil;

namespace AspectInjector.Contexts
{
    public class ProcessingContext
    {
        public AssemblyDefinition Assembly { get; internal set; }

        public ILogger Log { get; set; }

        public IAssemblyResolver Resolver { get; set; }
    }
}