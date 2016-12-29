using AspectInjector.Broker;
using System;

namespace AspectInjector.Tests.InterfaceInjection.NewTest
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [AspectDefinition(typeof(FreezableAspect), AccessModifierFilter = AccessModifiers.Public)]
    public class FreezableAttribute : Attribute
    {
    }

    [AdviceInterfaceProxy(typeof(IFreezable))]
    internal class FreezableAspect : IFreezable
    {
        public bool IsFrozen { get; private set; }

        void IFreezable.Freeze()
        {
            IsFrozen = true;
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void CheckIfFrozen([AdviceArgument(AdviceArgumentSource.Instance)] object obj)
        {
            if (obj is IFreezable)
            {
                if (((IFreezable)obj).IsFrozen)
                    throw new InvalidOperationException("Attemting to modify frozen object.");
            }
        }
    }

    [Freezable]
    public class TestClass
    {
        public string Text { get; set; }
    }

    public interface IFreezable
    {
        void Freeze();

        bool IsFrozen { get; }
    }
}