using System.Collections.Generic;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal class AspectInjector
    {
        private List<IAspectInjector> injectors = new List<IAspectInjector>();

        public AspectInjector()
        {
            injectors.Add(new CustomAspectInjector());
            injectors.Add(new PropertyChangedAspectInjector());
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
