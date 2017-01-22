using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Services
{
    public abstract class EffectExtractorBase : ServiceBase
    {
        public EffectExtractorBase(Logger logger) : base(logger)
        {
        }

        public abstract IEnumerable<Effect> Extract(ICustomAttributeProvider host);
    }
}