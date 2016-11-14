using AspectInjector.Contexts;
using AspectInjector.Contracts;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Defaults
{
    public abstract class DefaultAdviceExtractorBase<T> : IAdviceExtractor<T>
        where T : IAdvice
    {
        protected ILogger Log { get; private set; }

        public abstract IEnumerable<T> ExtractAdvices(ModuleDefinition module);

        public void Init(ProcessingContext context)
        {
            Log = context.Log;
        }
    }
}