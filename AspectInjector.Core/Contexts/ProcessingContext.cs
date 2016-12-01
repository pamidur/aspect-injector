using AspectInjector.Core.Fluent;
using Mono.Cecil;

namespace AspectInjector.Core.Contexts
{
    public class ProcessingContext
    {
        public AssemblyDefinition Assembly { get; internal set; }

        public IAssemblyResolver Resolver { get; internal set; }

        public ServicesContext Services { get; internal set; }

        public EditorFactory Editors { get; internal set; }
    }
}