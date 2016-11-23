using System;

namespace AspectInjector.Core.Fluent
{
    public class FluentInterfaceImplementation
    {
        public FluentInterfaceImplementation ImplementMethod(Action<FluentMethod> action)
        {
            return this;
        }
    }
}