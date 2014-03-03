using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdviceAttribute : Attribute
    {
        public InjectionTarget Targets { get; set; }
        public InjectionPoint Points { get; set; }
    }
}
