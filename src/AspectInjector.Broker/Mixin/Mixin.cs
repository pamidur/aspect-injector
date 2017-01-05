using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Mixes in an interface implementation trough creating proxy methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class Mixin : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mixin" /> class.
        /// </summary>
        /// <param name="interface">Interface to implement.</param>
        public Mixin(Type @interface)
        {
        }
    }
}