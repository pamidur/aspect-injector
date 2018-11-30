using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class Injection : IEquatable<Injection>
    {
        public IMemberDefinition Target { get; internal set; }

        public uint Priority { get; internal set; }

        public AspectDefinition Source { get; internal set; }

        public Effect Effect { get; internal set; }

        public List<ICustomAttribute> Triggers { get; internal set; } = new List<ICustomAttribute>();        

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