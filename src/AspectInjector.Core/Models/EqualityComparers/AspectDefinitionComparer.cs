using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using System.Collections.Generic;

namespace AspectInjector.Core.EqualityComparers
{
    internal class AspectDefinitionComparer : IEqualityComparer<AspectDefinition>
    {
        public bool Equals(AspectDefinition x, AspectDefinition y)
        {
            return x.Host.GetFQN() == y.Host.GetFQN();
        }

        public int GetHashCode(AspectDefinition ad)
        {
            return ad.Host.FullName.GetHashCode();
        }

        public static AspectDefinitionComparer Instance { get; } = new AspectDefinitionComparer();
    }
}