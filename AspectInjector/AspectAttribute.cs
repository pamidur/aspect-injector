using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class AspectAttribute : Attribute
    {
        public AspectAttribute(Type aspectType)
        {
            Type = aspectType;
        }

        public Type Type { get; private set; }
    }
}