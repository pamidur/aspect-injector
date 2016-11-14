using AspectInjector.Contexts;
using AspectInjector.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Contracts
{
    public interface IAdviceExtractor<IAdvice>
    {
        void Init(ProcessingContext context);

        IEnumerable<IAdvice> ExtractAdvices(ModuleDefinition module);
    }
}