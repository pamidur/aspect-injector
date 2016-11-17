using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Defaults
{
    public abstract class DefaultAdviceExtractorBase<T> : IAdviceExtractor
        where T : Advice
    {
        protected ILogger Log { get; private set; }

        protected abstract IEnumerable<T> ExtractAdvices(ModuleDefinition module);

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
        }

        IEnumerable<Advice> IAdviceExtractor.ExtractAdvices(ModuleDefinition module)
        {
            return ExtractAdvices(module).Cast<Advice>();
        }
    }
}