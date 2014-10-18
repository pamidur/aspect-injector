using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AspectFactoryAttribute : Attribute
    {
    }
}