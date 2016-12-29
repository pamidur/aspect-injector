using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Models
{
    public class AspectUsage<TTarget> : AspectUsage, IEquatable<AspectUsage>
        where TTarget : class, ICustomAttributeProvider
    {
        public AspectUsage()
        {
            TargetKind = (AspectTargetKind)Enum.Parse(typeof(AspectTargetKind), typeof(TTarget).Name);
        }

        public TTarget Target { get; set; }

        public bool Equals(AspectUsage other)
        {
            return other is AspectUsage<TTarget>
                && other.InjectionHost.GetFQN() == InjectionHost.GetFQN()
                && ((AspectUsage<TTarget>)other).Target == Target;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is AspectUsage<TTarget> && Equals((AspectUsage<TTarget>)obj);
        }

        public override int GetHashCode()
        {
            return InjectionHost.GetFQN().GetHashCode();
        }
    }

    public abstract class AspectUsage
    {
        public AspectTargetKind TargetKind { get; protected set; }
        public MethodDefinition InjectionHostFactory { get; internal set; }
        public AspectCreationScope Scope { get; internal set; }
        public uint Priority { get; internal set; }
        public TypeReference InjectionHost { get; internal set; }
        public IEnumerable<CustomAttribute> RoutableData { get; internal set; }
    }
}