using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Defines method parameter as a special advice parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class Argument : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Argument" /> class.
        /// </summary>
        /// <param name="source">Specifies source of advice argument.</param>
        public Argument(Source source)
        {
            Source = source;
        }

        /// <summary>
        /// Advice argument source used to populate method parameter.
        /// </summary>
        public Source Source { get; }
    }
}
