using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectExtractor
    {
        IReadOnlyCollection<Effect> Extract(ICustomAttributeProvider host);
    }
}