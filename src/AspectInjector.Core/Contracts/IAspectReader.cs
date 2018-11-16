using System.Collections.Generic;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectReader
    {
        IReadOnlyCollection<AspectDefinition> ReadAll(AssemblyDefinition assembly);
        AspectDefinition Read(TypeDefinition type);
    }
}