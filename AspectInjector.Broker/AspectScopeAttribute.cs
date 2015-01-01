using System;

namespace AspectInjector.Broker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AspectScopeAttribute : Attribute
    {
        public AspectScope Scope { get; private set; }

        public AspectScopeAttribute(AspectScope scope)
        {
            Scope = scope;
        }
    }
}
