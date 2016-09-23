using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    public sealed class AspectAttribute : Attribute
    {
        public Type Type { get; private set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }

        public AspectAttribute(Type aspectType)
        {
            Type = aspectType;
        }
    }
}