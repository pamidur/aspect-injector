using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IInjectionCollector : IInitializable
    {
        IEnumerable<Injection> Collect(AssemblyDefinition assembly);
    }
}