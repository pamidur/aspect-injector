using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdviceAttribute : Attribute
    {
        public InjectionTarget Targets { get; set; }
        public InjectionPoint Points { get; set; }
    }
}
