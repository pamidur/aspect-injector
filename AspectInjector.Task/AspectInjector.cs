using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask
{
    internal class AspectInjector
    {
        private List<IAspectInjector> injectors = new List<IAspectInjector>();

        public AspectInjector()
        {
            injectors.Add(new AspectBodyInterfaceProxyInjector());
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

            //var reference = assembly.MainModule.AssemblyReferences.FirstOrDefault(ar => ar.Name == "AspectInjector");
            //if (reference != null)
            //    assembly.MainModule.AssemblyReferences.Remove(reference);
        }
    }
}