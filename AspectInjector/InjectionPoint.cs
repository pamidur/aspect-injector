using System;

namespace AspectInjector
{
    [Flags]
    public enum InjectionPoint
    {
        Before = 1,
        After = 2
    }
}
