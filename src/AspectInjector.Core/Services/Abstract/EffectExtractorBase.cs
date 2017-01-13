using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Services.Abstract
{
    public abstract class EffectExtractorBase : ServiceBase
    {
        public EffectExtractorBase(string prefix, ILogger logger) : base(prefix, logger)
        {
        }

        protected abstract IEnumerable<Effect> Extract(ICustomAttributeProvider host);
    }

    public abstract class EffectExtractorBase<T> : EffectExtractorBase
        where T : Effect
    {
        public EffectExtractorBase(string prefix, ILogger logger) : base(prefix, logger)
        {
        }

        protected override IEnumerable<Effect> Extract(ICustomAttributeProvider host)
        {
            var effects = new List<Effect>();

            foreach (var attribute in host.CustomAttributes)
            {
                if (attribute.AttributeType.IsTypeOf(typeof(Broker.Mixin)))
                {
                    effects.Add(ReadEffect(attribute));
                    host.CustomAttributes.Remove(attribute);
                }
            }

            return effects;
        }

        protected abstract bool HasEffect

        protected abstract T ReadEffect(CustomAttribute attribute);
    }
}