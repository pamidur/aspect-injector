using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public sealed class InjectionDefinition : IEquatable<InjectionDefinition>
    {
        public IMemberDefinition Target { get; internal set; }

        public uint Priority { get; internal set; }

        public AspectDefinition Source { get; internal set; }

        public Effect Effect { get; internal set; }

        public List<CustomAttribute> Triggers { get; internal set; } = new List<CustomAttribute>();

        public override bool Equals(object obj)
        {
            return obj is InjectionDefinition id && Equals(id);
        }

        public bool Equals(InjectionDefinition other)
        {
            return Source.Host.FullName == other.Source.Host.FullName
                && Target == other.Target
                && Effect.Equals(other.Effect);
        }

        public override int GetHashCode()
        {
            return Source.Host.FullName.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Effect.ToString()} => {Target.MetadataToken.TokenType.ToString()} ::{Target.FullName}";
        }
    }
}