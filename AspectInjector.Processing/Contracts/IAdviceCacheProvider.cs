using AspectInjector.Core.Contexts;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAdviceCacheProvider : IInitializable
    {
        IEnumerable<T> GetAdvices<T>(TypeReference type) where T : IAdvice;

        void StoreAdvices<T>(ModuleDefinition toModule, IEnumerable<T> advices) where T : IAdvice;
    }
}