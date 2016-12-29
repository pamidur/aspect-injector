using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IInjectionReader : IInitializable
    {
        IEnumerable<Injection> ReadInjections(ModuleDefinition module);

        void Cleanup(ModuleDefinition module);
    }
}