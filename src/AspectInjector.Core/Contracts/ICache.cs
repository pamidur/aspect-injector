using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface ICache
    {
        AspectDefinition GetAspectDefinition(TypeReference host);

        IEnumerable<CutSpecDefinition> GetCutSpecDefinitions(TypeReference host);

        void Cache(IEnumerable<AspectDefinition> aspects);

        void Cache(IEnumerable<CutDefinition> cuts);
    }
}