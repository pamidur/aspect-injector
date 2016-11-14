using Mono.Cecil;
using System.Runtime.InteropServices;

namespace AspectInjector
{
    [ComVisible(false)]
    public class StrictAssemblyResolver : DefaultAssemblyResolver
    {
        public StrictAssemblyResolver()
        {
        }

        public StrictAssemblyResolver(string[] assemblyPaths)
        {
            RegisterAssemblies(assemblyPaths);
        }

        public void RegisterAssembly(string assemblyPath)
        {
            var assmDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            RegisterAssembly(assmDefinition);
        }

        public void RegisterAssemblies(string[] assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
                RegisterAssembly(assemblyPath);
        }
    }
}