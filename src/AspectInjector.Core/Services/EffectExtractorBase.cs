using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Effect> Extract(ICustomAttributeProvider host)
        {
            var source = host as TSource;

            if (source != null)
                return Extract(source);

            return Enumerable.Empty<Effect>();
        }

        protected abstract IEnumerable<TEffect> Extract(TSource host);
    }
}