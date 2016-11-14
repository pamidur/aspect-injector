using AspectInjector.Contexts;
using AspectInjector.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Contracts
{
    public interface IAspectExtractor
    {
        void Init(ProcessingContext context);

        IEnumerable<Aspect> ExtractAspects(ModuleDefinition module);
    }
}