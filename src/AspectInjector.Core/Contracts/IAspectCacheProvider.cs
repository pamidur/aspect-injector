using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectCacheProvider : IInitializable
    {
        IEnumerable<Effect> GetEffects(TypeReference aspect);

        void CacheEffects(ModuleDefinition toModule, IEnumerable<Effect> effects);
    }
}