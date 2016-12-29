using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class Injection<TTarget> : Injection, IEquatable<Injection>
        where TTarget : class, ICustomAttributeProvider
    {
        public Injection()
        {
            TargetKind = (InjectionTargetType)Enum.Parse(typeof(InjectionTargetType), typeof(TTarget).Name);
        }

        public TTarget Target { get; set; }

        public bool Equals(Injection other)
        {
            return other is Injection<TTarget>
                && other.Aspect.GetFQN() == Aspect.GetFQN()
                && ((Injection<TTarget>)other).Target == Target;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Injection<TTarget> && Equals((Injection<TTarget>)obj);
        }

        public override int GetHashCode()
        {
            return Aspect.GetFQN().GetHashCode();
        }
    }

    public abstract class Injection
    {
        public InjectionTargetType TargetKind { get; protected set; }
        public MethodDefinition AspectFactory { get; internal set; }
        public AspectCreationScope Scope { get; internal set; }
        public uint Priority { get; internal set; }
        public TypeReference Aspect { get; internal set; }
        public IEnumerable<CustomAttribute> RoutableData { get; internal set; }
    }
}