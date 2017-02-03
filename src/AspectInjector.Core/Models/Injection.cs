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

        public bool Equals(Injection other)
        {
            return Source.Host.GetFQN() == other.Source.Host.GetFQN()
                && Target == other.Target
                && Effect.Equals(other.Effect);
        }

        public override int GetHashCode()
        {
            return Source.Host.GetFQN().GetHashCode();
        }
    }
}