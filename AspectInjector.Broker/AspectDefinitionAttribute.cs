using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AspectDefinitionAttribute : AspectAttributeBase
    {
        public AspectDefinitionAttribute(Type aspectType)
        {
            Type = aspectType;
        }
    }
}