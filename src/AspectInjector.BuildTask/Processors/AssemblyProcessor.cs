using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Processors
{
    public class AssemblyProcessor
    {
        private readonly IEnumerable<IModuleProcessor> _processors;

        public AssemblyProcessor(IEnumerable<IModuleProcessor> processors)
        {
            _processors = processors;
        }

        public void Process(AssemblyDefinition assembly)
        {
            foreach (var processor in _processors)
            {
                processor.ProcessModule(assembly.MainModule);
            }
        }
    }
}