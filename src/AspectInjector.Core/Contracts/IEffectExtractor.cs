using System.Collections.Generic;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectExtractor
    {
        IEnumerable<Effect> Extract(ICustomAttributeProvider host);
    }
}