using AspectInjector.Core.Models;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IInjectionCollector : IInitializable
    {
        IEnumerable<Injection> CollectInjections(IEnumerable<CutDefinition> cuts);
    }
}