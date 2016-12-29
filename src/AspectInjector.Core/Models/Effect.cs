using System;

namespace AspectInjector.Core.Models
{
    public abstract class Effect : IEquatable<Effect>
    {
        public uint Priority { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Effect)obj);
        }

        public abstract bool IsApplicableFor(Injection aspect);

        public abstract bool Equals(Effect other);

        public abstract override int GetHashCode();
    }
}