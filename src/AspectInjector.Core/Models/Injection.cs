using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class Injection
    {
        public ICustomAttributeProvider Target { get; set; }

        public uint Priority { get; internal set; }

        public TypeReference Source { get; internal set; }

        public Effect Effect { get; internal set; }

        public IEnumerable<CustomAttribute> RoutableData { get; internal set; }

        public bool Equals(Injection other)
        {
            return other is Injection<TTarget>
                && other.Source.GetFQN() == Source.GetFQN()
                && ((Injection<TTarget>)other).Target == Target;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Injection<TTarget> && Equals((Injection<TTarget>)obj);
        }

        public override int GetHashCode()
        {
            return Source.GetFQN().GetHashCode();
        }
    }
}