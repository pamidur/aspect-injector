using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdviceAttribute : Attribute
    {
        public InjectionTargets Targets { get; set; }
        public InjectionPoints Points { get; set; }
    }
}
