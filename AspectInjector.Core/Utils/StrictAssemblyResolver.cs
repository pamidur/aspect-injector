using Mono.Cecil;
using System.Runtime.InteropServices;

namespace AspectInjector.Core.Utils
{
    [ComVisible(false)]
    public class StrictAssemblyResolver : CachedAssemblyResolver
    {
        public void RegisterAssembly(string assemblyPath)
        {
            var assmDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            RegisterAssembly(assmDefinition);
        }
    }
}