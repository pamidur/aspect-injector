using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IInjectionCacheProvider : IInitializable
    {
        IEnumerable<Injection> GetInjections(TypeReference type);

        void StoreInjections(ModuleDefinition toModule, IEnumerable<Injection> injections);
    }
}