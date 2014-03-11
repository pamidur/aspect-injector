using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class AspectAttribute : Attribute
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