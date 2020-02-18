using FluentIL.Logging;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public abstract class Effect : IEquatable<Effect>
    {
        public uint Priority { get; protected set; }

        public abstract bool Equals(Effect other);             

        public override bool Equals(object obj)
        {
            return obj is Effect ef && Equals(ef);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public abstract bool IsApplicableFor(IMemberDefinition target);

        public abstract bool Validate(AspectDefinition aspect, ILogger log);

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}