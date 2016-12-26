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
            TargetKind = (AspectTargetKind)Enum.Parse(typeof(AspectTargetKind), typeof(TTarget).Name);
        }

        public TTarget Target { get; set; }

        public bool Equals(Aspect other)
        {
            return other is Aspect<TTarget>
                && other.InjectionHost.GetFQN() == InjectionHost.GetFQN()
                && ((Aspect<TTarget>)other).Target == Target;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Aspect<TTarget> && Equals((Aspect<TTarget>)obj);
        }

        public override int GetHashCode()
        {
            return InjectionHost.GetFQN().GetHashCode();
        }
    }

    public abstract class Aspect
    {
        public AspectTargetKind TargetKind { get; protected set; }
        public MethodDefinition InjectionHostFactory { get; internal set; }
        public AspectScope Scope { get; internal set; }
        public uint Priority { get; internal set; }
        public TypeReference InjectionHost { get; internal set; }
        public IEnumerable<CustomAttribute> RoutableData { get; internal set; }
    }
}