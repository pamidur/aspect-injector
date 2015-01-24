using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AdviceAttribute : Attribute
    {
        public InjectionPoints Points { get; private set; }

        public InjectionTargets Targets { get; private set; }

        public AdviceAttribute(InjectionPoints points, InjectionTargets targets)
        {
            Points = points;
            Targets = targets;
        }
    }
}
