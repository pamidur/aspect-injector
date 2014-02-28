using System;

namespace AspectInjector.Broker
{
    [Flags]
    public enum InjectionPoint
    {
        Before = 1,
        After = 2
    }
}
