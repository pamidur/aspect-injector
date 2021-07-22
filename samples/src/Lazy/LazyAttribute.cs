using AspectInjector.Broker;
using System;

namespace Aspects.Lazy
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Injection(typeof(LazyAspect))]
    public sealed class LazyAttribute : Attribute
    {

    }
}
