using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public abstract class Effect : IEquatable<Effect>
    {
        public TypeReference Aspect { get; set; }

        public uint Priority { get; protected set; }

        public bool Equals(Effect other)
        {
            return other.Aspect.GetFQN() == Aspect.GetFQN() && IsEqualTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Effect)obj);
        }

        public override int GetHashCode()
        {
            return Aspect.GetFQN().GetHashCode();
        }

        public bool IsApplicableFor(Injection aspect)
        {
            return aspect.Aspect.GetFQN() == Aspect.GetFQN() && IsApplicableForAspect(aspect);
        }

        protected virtual bool IsApplicableForAspect(Injection aspect)
        {
            return true;
        }

        protected abstract bool IsEqualTo(Effect other);
    }
}