using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Services
{
    public abstract class EffectExtractorBase<TSource, TEffect> : IEffectExtractor
        where TSource : class, ICustomAttributeProvider
        where TEffect : Effect
    {
        protected readonly ILogger _log;

        public EffectExtractorBase(ILogger logger)
        {
            _log = logger;
        }

        public IReadOnlyCollection<Effect> Extract(ICustomAttributeProvider host)
        {
            var source = host as TSource;

            if (source != null)
                return Extract(source);

            return new List<Effect>();
        }

        protected abstract IReadOnlyCollection<TEffect> Extract(TSource host);
    }
}