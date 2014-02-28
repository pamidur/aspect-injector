using System;

namespace AspectInjector.Broker
{
    [Flags]
    public enum InjectionTarget
    {
        Constructor = 1,
        Method = 2,
        Getter = 4,
        Setter = 8
    }
}
