using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Configures aspect usage scenarios.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AspectUsage : Attribute
    {
        /// <summary>
        /// Scope in which aspect is instantiated.
        /// </summary>
        public CreationScope Scope { get; set; }

        /// <summary>
        /// Type that is used as aspect factory. Type should contain <code>public static object GetInstance(Type)</code> method. <code>null</code> represents original parameter-less constructor.
        /// </summary>
        public Type Factory { get; set; }

        /// <summary>
        /// Advice creation scope enumeration.
        /// </summary>
        public enum CreationScope
        {
            /// <summary>
            /// Aspect is created and used as singleton. Default value.
            /// </summary>
            Global,

            /// <summary>
            /// Instance of an aspect is created per target class instance.
            /// </summary>
            PerInstance
        }
    }
}