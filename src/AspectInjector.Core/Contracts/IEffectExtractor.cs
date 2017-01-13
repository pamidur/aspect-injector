using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectExtractor
    {
        Type EffectType { get; }

        IEnumerable<Effect> Extract(ICustomAttributeProvider host);
    }
}