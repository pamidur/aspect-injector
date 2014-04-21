using System.Runtime.InteropServices;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    [ComVisible(false)]
    public class StrictAssemblyResolver : DefaultAssemblyResolver
    {
        public void RegisterAssembly(string assemblyPath)
        {
            var assmDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            RegisterAssembly(assmDefinition);
        }
    }
}