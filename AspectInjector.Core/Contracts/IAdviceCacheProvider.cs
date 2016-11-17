using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAdviceCacheProvider : IInitializable
    {
        IEnumerable<Advice> GetAdvices(TypeReference type);

        void StoreAdvices(ModuleDefinition toModule, IEnumerable<Advice> advices);
    }
}