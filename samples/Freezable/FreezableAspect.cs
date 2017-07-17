using AspectInjector.Broker;
using System;

namespace AspectInjector.SampleApps.Freezable
{
    [Aspect(Aspect.Scope.PerInstance)]
    [Mixin(typeof(IFreezable))]
    class FreezableAspect : IFreezable
    {
        public bool IsFrozen { get; set; }

        public void Freeze()
        {
            IsFrozen = true;
        }

        [Advice(Advice.Type.Before, Advice.Target.Setter)]
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
