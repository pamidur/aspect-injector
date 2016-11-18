using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class Aspect<TTarget> : Aspect, IEquatable<Aspect>
        where TTarget : class, ICustomAttributeProvider
    {
        public Aspect()
        {
            TargetType = (AspectTargetType)Enum.Parse(typeof(AspectTargetType), typeof(TTarget).Name);
        }

        public TTarget Target { get; set; }

        public bool Equals(Aspect other)
        {
            return other is Aspect<TTarget>
                && other.AdviceHost.GetFQN() == AdviceHost.GetFQN()
                && ((Aspect<TTarget>)other).Target == Target;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Aspect<TTarget> && Equals((Aspect<TTarget>)obj);
        }

        public override int GetHashCode()
        {
            return AdviceHost.GetFQN().GetHashCode();
        }
    }

    public abstract class Aspect
    {
        public AspectTargetType TargetType { get; protected set; }

        public TypeReference AdviceHost { get; set; }

        public IEnumerable<CustomAttribute> RoutableData { get; set; }
    }
}