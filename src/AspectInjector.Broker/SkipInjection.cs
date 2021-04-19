using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// When applied to a target, suppresses injection of any aspects to it. 
    /// If applied to an interface or an interface method, suppresses injection to any of the corresponding implementations.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Method | 
        AttributeTargets.Constructor | 
        AttributeTargets.Property | 
        AttributeTargets.Event, 
        AllowMultiple = false)]
    public class SkipInjection : Attribute
    {
    }
}
