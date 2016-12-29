using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class Incut : AspectBase
    {
        public Incut(Type aspectType)
        {
        }
    }
}