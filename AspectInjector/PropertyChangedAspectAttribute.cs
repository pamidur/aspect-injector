using System;

namespace AspectInjector
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class PropertyChangedAspectAttribute : Attribute
    {
    }
}
