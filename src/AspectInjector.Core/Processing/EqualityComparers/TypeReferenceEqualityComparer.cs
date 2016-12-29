using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Processing.EqualityComparers
{
    internal class TypeReferenceEqualityComparer : IEqualityComparer<TypeReference>
    {
        public bool Equals(TypeReference x, TypeReference y)
        {
            return x.GetFQN() == y.GetFQN();
        }

        public int GetHashCode(TypeReference obj)
        {
            return obj.Resolve().Module.Assembly.Name.Name.GetHashCode();
        }

        public static TypeReferenceEqualityComparer Instance { get; } = new TypeReferenceEqualityComparer();
    }
}