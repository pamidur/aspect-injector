using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InterfaceProxyInjectionAttribute : Attribute
    {
        public InterfaceProxyInjectionAttribute(Type @interface)
        {
            Interface = @interface;
        }

        public Type Interface { get; private set; }
    }
}