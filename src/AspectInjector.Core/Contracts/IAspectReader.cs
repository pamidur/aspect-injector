using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectReader : IInitializable
    {
        IEnumerable<Effect> ReadEffects(ModuleDefinition module);

        void Cleanup(ModuleDefinition module);
    }
}