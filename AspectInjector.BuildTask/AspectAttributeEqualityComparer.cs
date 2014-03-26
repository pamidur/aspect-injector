using System.Collections.Generic;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal class AspectAttributeEqualityComparer : IEqualityComparer<CustomAttribute>
    {
        public bool Equals(CustomAttribute x, CustomAttribute y)
        {
            return object.Equals(x.ConstructorArguments, y.ConstructorArguments) &&
                object.Equals(x.Properties, y.Properties);
        }

        public int GetHashCode(CustomAttribute obj)
        {
            return obj.GetHashCode();
        }
    }
}
