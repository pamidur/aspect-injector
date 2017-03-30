using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IAssetsCache
    {
        void Cache(AspectDefinition aspect);

        void FlushCache(AssemblyDefinition assembly);

        AspectDefinition ReadAspect(TypeDefinition type);
    }
}