using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using static AspectInjector.Broker.AspectUsage;

namespace AspectInjector.Core.Models
{
    public class AspectDefinition : IEquatable<AspectDefinition>
    {
        public TypeReference Host { get; set; }

        public List<Effect> Effects { get; set; }

        public CreationScope Scope { get; set; }

        public bool Equals(AspectDefinition other)
        {
            return other.Host.GetFQN() == Host.GetFQN();
        }

        public override int GetHashCode()
        {
            return Host.GetFQN().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

            return Equals((AspectDefinition)obj);
        }
    }
}