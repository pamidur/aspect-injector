using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public abstract class Advice : IEquatable<Advice>
    {
        public TypeReference HostType { get; set; }

        public bool Equals(Advice other)
        {
            return other.HostType.GetFQN() == HostType.GetFQN() && IsEqualTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Advice)obj);
        }

        public override int GetHashCode()
        {
            return HostType.GetFQN().GetHashCode();
        }

        public bool IsApplicableFor(Aspect aspect)
        {
            return aspect.AdviceHost.GetFQN() == HostType.GetFQN() && IsApplicableForAspect(aspect);
        }

        protected virtual bool IsApplicableForAspect(Aspect aspect)
        {
            return true;
        }

        protected abstract bool IsEqualTo(Advice other);
    }
}