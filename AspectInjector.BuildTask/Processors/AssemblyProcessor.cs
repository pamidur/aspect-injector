using System.Collections.Generic;
using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Processors
{
    internal class AssemblyProcessor
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