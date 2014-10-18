using System;

namespace AspectInjector.Broker
{
    [Flags]
    public enum InjectionTargets
    {
        Constructor = 1,
        Method = 2,
        Getter = 4,
        Setter = 8,
        EventAdd = 16,
        EventRemove = 32
    }
}