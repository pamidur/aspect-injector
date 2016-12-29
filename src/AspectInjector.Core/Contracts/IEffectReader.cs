using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectReader : IInitializable
    {
        IEnumerable<Effect> ReadEffects(TypeDefinition aspectHost);

        void Cleanup(ModuleDefinition module);
    }
}