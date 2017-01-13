using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services.Abstract;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Services.Extraction
{
    public abstract class EffectExtractorBase : ServiceBase
    {
        public EffectExtractorBase(Logger logger) : base(logger)
        {
        }

        protected abstract IEnumerable<Effect> Extract(ICustomAttributeProvider host);
    }

    public abstract class EffectExtractorBase<T> : EffectExtractorBase
        where T : Effect
    {
        public EffectExtractorBase(Logger logger) : base(logger)
        {
        }

        protected override IEnumerable<Effect> Extract(ICustomAttributeProvider host)
        {
            var effects = new List<Effect>();

            foreach (var attribute in host.CustomAttributes)
            {
                if (HasEffect(attribute))
                {
                    effects.Add(ReadEffect(attribute));
                    host.CustomAttributes.Remove(attribute);
                }
            }

            return effects;
        }

        protected abstract bool HasEffect(CustomAttribute attribute);

        protected abstract T ReadEffect(CustomAttribute attribute);
    }
}