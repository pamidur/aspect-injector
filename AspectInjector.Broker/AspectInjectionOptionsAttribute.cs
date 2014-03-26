using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectInjectionOptionsAttribute : Attribute
    {
        public InjectionScope Scope { get; set; }
    }
}