using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectExtractor : IInitializable
    {
        IEnumerable<Aspect> ExtractAspects(AssemblyDefinition assembly);
    }
}