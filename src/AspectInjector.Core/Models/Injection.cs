using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Models
{
    public class Injection : IEquatable<Injection>
    {
        public IMemberDefinition Target { get; set; }

        public uint Priority { get; internal set; }

        public AspectDefinition Source { get; internal set; }

        public Effect Effect { get; internal set; }

        public bool Equals(Injection other)
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