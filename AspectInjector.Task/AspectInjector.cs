using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask
{
    internal class AspectInjector
    {
        private List<IAspectInjector> injectors = new List<IAspectInjector>();

        public AspectInjector()
        {
            injectors.Add(new CustomAspectInjector());
            injectors.Add(new PropertyAspectInjector());
            injectors.Add(new MethodAspectInjector());
        }

        public void Process(AssemblyDefinition assembly)
        {
            foreach (var injector in injectors)
            {
                injector.ProcessModule(assembly.MainModule);
            }
        }
    }
}