using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Processing
{
    public class AspectExtractor : IAspectExtractor
    {
        public IEnumerable<Aspect> ExtractAspects(AssemblyDefinition assembly)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Aspect> ExtractAspects(ModuleDefinition module)
        {
            return new List<Aspect>();
        }

        public void Init(ProcessingContext context)
        {
        }
    }
}