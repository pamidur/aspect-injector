using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Configures aspect usage scenarios.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class Aspect : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Aspect" /> class.
        /// </summary>
        /// <param name="scope">Scope in which aspect is instantiated.</param>
        public Aspect(Scope scope)
        {
            Scope = scope;
        }

        /// <summary>
        /// Type that is used as aspect factory. Type should contain <code>public static object GetInstance(Type)</code> method. <code>null</code> represents original parameter-less constructor.
        /// </summary>
        public Type Factory { get; set; }
        /// <summary>
        /// Scope in which aspect is instantiated.
        /// </summary>
        public Scope Scope { get; }
    }
}