using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal interface IModuleProcessor
    {
        void ProcessModule(ModuleDefinition module);
    }
}