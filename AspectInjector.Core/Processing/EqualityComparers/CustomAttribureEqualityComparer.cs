using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Processing.EqualityComparers
{
    internal class CustomAttribureEqualityComparer : IEqualityComparer<CustomAttribute>
    {
        public bool Equals(CustomAttribute x, CustomAttribute y)
        {
            return x.GetBlob().SequenceEqual(y.GetBlob());
        }

        public int GetHashCode(CustomAttribute obj)
        {
            return obj.AttributeType.GetFQN().GetHashCode();
        }

        public static CustomAttribureEqualityComparer Instance { get; } = new CustomAttribureEqualityComparer();
    }
}