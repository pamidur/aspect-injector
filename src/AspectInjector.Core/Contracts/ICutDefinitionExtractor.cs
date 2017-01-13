using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Contracts
{
    public interface ICutDefinitionExtractor
    {
        IEnumerable<CutDefinition> Extract(ICustomAttributeProvider target);
    }
}