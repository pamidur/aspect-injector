using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property)]
    public class AspectAttribute : Attribute
    {
        public Type Type { get; set; }
    }
}