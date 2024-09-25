using AspectInjector.Broker;
using System;
using System.Diagnostics;

namespace Aspects.Freezable
{
    [Aspect(Scope.PerInstance)]
    [Mixin(typeof(IFreezable))]
    [Injection(typeof(FreezableAttribute))]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class FreezableAttribute : Attribute, IFreezable
    {
        public bool IsFrozen { get; set; }

        public void Freeze()
        {
            IsFrozen = true;
        }

        [Advice(Kind.Before, Targets = Target.Public | Target.Setter)]
        [DebuggerHidden]
        [DebuggerStepThrough]
        public void CheckIfFrozen(
            [Argument(Source.Name)] string name,
            [Argument(Source.Type)] Type type
            )
        {
            if (IsFrozen)
                throw new InvalidOperationException($"Attempt to modify '{type.Name}.{name}' of a frozen object.");
        }
    }

    public interface IFreezable
    {
        bool IsFrozen { get; }
        void Freeze();
    }
}
