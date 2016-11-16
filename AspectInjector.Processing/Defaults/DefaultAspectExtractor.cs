using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Defaults
{
    public class DefaultAspectExtractor : IAspectExtractor
    {
        public IEnumerable<Aspect> ExtractAspects(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }

        public void Init(ProcessingContext context)
        {
        }
    }
}