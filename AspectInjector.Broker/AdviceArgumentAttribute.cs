using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class AdviceArgumentAttribute : Attribute
    {
        public AdviceArgumentSource Source { get; set; }
    }
}