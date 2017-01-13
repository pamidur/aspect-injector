using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IAssemblyProcessor
    {
        void ProcessAssembly(AssemblyDefinition assembly);
    }
}