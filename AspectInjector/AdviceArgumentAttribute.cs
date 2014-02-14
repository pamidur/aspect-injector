using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class AdviceArgumentAttribute : Attribute
    {
        public AdviceArgumentSource Source { get; set; }
    }
}