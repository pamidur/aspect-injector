using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AspectDefinition : AspectBase
    {
        public AspectDefinition(Type aspectType)
        {
        }
    }
}