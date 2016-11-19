using System;

namespace AspectInjector.Core.Fluent
{
    public class FluentInterfaceImplementation
    {
        public FluentInterfaceImplementation ImplementMethod(Action<FluentMethodConstructor> action)
        {
            return this;
        }
    }
}