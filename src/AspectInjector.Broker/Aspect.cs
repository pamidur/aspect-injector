using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class Aspect : AspectBase
    {
        public Aspect(Type aspectType)
        {
        }
    }
}