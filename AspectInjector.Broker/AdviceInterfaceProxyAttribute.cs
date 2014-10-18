using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AdviceInterfaceProxyAttribute : Attribute
    {
        public AdviceInterfaceProxyAttribute(Type @interface)
        {
            Interface = @interface;
        }

        public Type Interface { get; private set; }
    }
}