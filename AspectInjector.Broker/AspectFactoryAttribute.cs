using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AspectFactoryAttribute : Attribute
    {
    }
}