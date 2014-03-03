using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    public class StrictAssemblyResolver : DefaultAssemblyResolver
    {
        public void RegisterAssembly(string assemblyPath)
        {
            var assmDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            RegisterAssembly(assmDefinition);
        }
    }
}