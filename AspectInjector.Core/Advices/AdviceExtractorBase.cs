using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Advices
{
    public abstract class AdviceExtractorBase<T> : IAdviceExtractor
        where T : IAdvice
    {
        protected ILogger Log { get; private set; }

        protected abstract IEnumerable<T> ExtractAdvices(ModuleDefinition module);

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
        }

        IEnumerable<IAdvice> IAdviceExtractor.ExtractAdvices(ModuleDefinition module)
        {
            return ExtractAdvices(module).Cast<IAdvice>();
        }
    }
}