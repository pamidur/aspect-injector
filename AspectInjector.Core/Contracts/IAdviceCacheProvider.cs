using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAdviceCacheProvider : IInitializable
    {
        IEnumerable<IAdvice> GetAdvices(TypeReference type);

        void StoreAdvices(ModuleDefinition toModule, IEnumerable<IAdvice> advices);
    }
}