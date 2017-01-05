using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectCache : IInitializable
    {
        AspectDefinition GetAspect(TypeReference host);

        void Cache(ModuleDefinition toModule, IEnumerable<AspectDefinition> aspect);

        //todo:: cache cutspecs
    }
}