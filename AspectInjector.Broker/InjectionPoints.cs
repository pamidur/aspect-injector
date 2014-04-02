using System;

namespace AspectInjector.Broker
{
    [Flags]
    public enum InjectionPoints
    {
        Before = 1,
        After = 2,
        Exception = 4
    }
}