using AspectInjector.Broker;
using System;
using System.Diagnostics;

namespace AspectInjector.SampleApps.Freezable
{
    [Aspect(Scope.PerInstance)]
    [Mixin(typeof(IFreezable))]
    [Injection(typeof(Freezable))]
    class Freezable : Attribute, IFreezable
    {
        public bool IsFrozen { get; set; }

        public void Freeze()
        {
            IsFrozen = true;
        }

        [Advice(Kind.Before, Targets = Target.Public | Target.Setter)]
        public void CheckIfFrozen()
        {
            if (IsFrozen)
                throw new InvalidOperationException("Attempt to modify frozen object.");
        }
    }

    internal interface IFreezable
    {
        bool IsFrozen { get; }
        void Freeze();
    }
}
