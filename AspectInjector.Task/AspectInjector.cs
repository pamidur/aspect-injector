using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class AspectInjector
    {
        private List<IAspectInjector> injectors = new List<IAspectInjector>();

        public AspectInjector()
        {
            injectors.Add(new PropertyAspectInjector());
            injectors.Add(new MethodAspectInjector());
        }

        public void Process(AssemblyDefinition assembly)
        {
            //System.Diagnostics.Debugger.Launch();

            foreach (var injector in injectors)
            {
                injector.ProcessModule(assembly.MainModule);
            }

            var moduleReference = assembly.MainModule.ModuleReferences.FirstOrDefault(ar => ar.Name == "AspectInjector");
            if (moduleReference != null)
                assembly.MainModule.ModuleReferences.Remove(moduleReference);

            var reference = assembly.MainModule.AssemblyReferences.FirstOrDefault(ar => ar.Name == "AspectInjector");
            if (reference != null)
                assembly.MainModule.AssemblyReferences.Remove(reference);
        }
    }
}