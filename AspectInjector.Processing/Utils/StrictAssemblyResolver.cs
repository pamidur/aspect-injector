using Mono.Cecil;
using System.IO;
using System.Runtime.InteropServices;

namespace AspectInjector.Core.Utils
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