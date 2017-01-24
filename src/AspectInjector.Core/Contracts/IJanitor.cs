using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IJanitor
    {
        void Cleanup(AssemblyDefinition assembly);
    }
}