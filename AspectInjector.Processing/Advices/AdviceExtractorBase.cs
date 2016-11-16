using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Advices
{
    public abstract class AdviceExtractorBase<T> : IAdviceExtractor<T>
        where T : IAdvice
    {
        protected ILogger Log { get; private set; }

        public abstract IEnumerable<T> ExtractAdvices(ModuleDefinition module);

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
        }
    }
}