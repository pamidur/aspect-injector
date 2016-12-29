using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public abstract class Injection : IEquatable<Injection>
    {
        public TypeReference HostType { get; set; }

        public uint Priority { get; protected set; }

        public bool Equals(Injection other)
        {
            return other.HostType.GetFQN() == HostType.GetFQN() && IsEqualTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Injection)obj);
        }

        public override int GetHashCode()
        {
            return HostType.GetFQN().GetHashCode();
        }

        public bool IsApplicableFor(AspectUsage aspect)
        {
            return aspect.InjectionHost.GetFQN() == HostType.GetFQN() && IsApplicableForAspect(aspect);
        }

        protected virtual bool IsApplicableForAspect(AspectUsage aspect)
        {
            return true;
        }

        protected abstract bool IsEqualTo(Injection other);
    }
}