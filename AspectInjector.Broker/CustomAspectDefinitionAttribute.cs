using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomAspectDefinitionAttribute : Attribute
    {
        public Type Type { get; private set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }

        public CustomAspectDefinitionAttribute(Type aspectType)
        {
            Type = aspectType;
        }
    }
}
