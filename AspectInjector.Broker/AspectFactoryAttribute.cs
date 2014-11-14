using System;

namespace AspectInjector.Broker
{
    public enum AspectScope
    {
        /// <summary>
        /// Instantinate aspect per instanse.
        /// </summary>
        PerInstanse = 0,

        /// <summary>
        /// Instantinate aspect per type. Aspects is disposed when application is shuting down.
        /// </summary>
        PerType = 1,

        /// <summary>
        /// Instantinate aspect using <see cref="AspectFactoryAttribute"/>
        /// </summary>
        CustomFactory = 2,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AspectScopeAttribute : Attribute
    {
        public AspectScope Scope { get; private set; }

        public AspectScopeAttribute(AspectScope scope)
        {
            Scope = scope;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AspectFactoryAttribute : Attribute
    {
    }
}