using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Defaults
{
    public class DefaultAspectExtractor : IAspectExtractor
    {
        public IEnumerable<Aspect> ExtractAspects(ModuleDefinition module)
        {
            return new List<Aspect>();
        }

        public void Init(ProcessingContext context)
        {
        }
    }
}