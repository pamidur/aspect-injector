using System.Collections.Generic;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IInjectionReader
    {
        IReadOnlyCollection<Injection> ReadAll(AssemblyDefinition assembly);
    }
}