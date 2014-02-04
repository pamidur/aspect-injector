using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal interface IAspectInjector
    {
        void ProcessModule(ModuleDefinition module);
    }
}
