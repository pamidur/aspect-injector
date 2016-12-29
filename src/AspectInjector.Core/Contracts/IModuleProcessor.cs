using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IAssemblyProcessor : IInitializable
    {
        void ProcessAssembly(AssemblyDefinition assembly);
    }
}