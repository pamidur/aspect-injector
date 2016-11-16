using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IAdviceExtractor : IInitializable
    {
        IEnumerable<IAdvice> ExtractAdvices(ModuleDefinition module);
    }
}