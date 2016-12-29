using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IncutSpecification : AspectBase
    {
        public IncutSpecification(Type aspectType)
        {
        }
    }
}