using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public class Injection : IEquatable<Injection>
    {
        public ICustomAttributeProvider Target { get; set; }
        public uint Priority { get; internal set; }
        public AspectDefinition Source { get; internal set; }
        public Effect Effect { get; internal set; }
        public TypeReference SourceReference { get; internal set; }

        public bool Equals(Injection other)
        {
            return SourceReference.GetFQN() == other.SourceReference.GetFQN()
                && Target == other.Target
                && Effect.Equals(other.Effect);
        }

        public override int GetHashCode()
        {
            return SourceReference.GetFQN().GetHashCode();
        }
    }
}