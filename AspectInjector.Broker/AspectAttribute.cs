using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    public sealed class AspectAttribute : AspectAttributeBase
    {
        public AspectAttribute(Type aspectType)
        {
            Type = aspectType;
        }
    }
}