using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CustomAspectAttribute : Attribute
    {
        public Type Type { get; set; }
    }
}
