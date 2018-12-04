using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Defines method as a special advice method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class Advice : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Advice" /> class.
        /// </summary>
        /// <param name="kind">Specifies when advice method should be called.</param>
        public Advice(Kind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Specifies what target members this method is injected to. <see cref="Target.Any" /> by default.
        /// </summary>
        public Target Targets { get; set; }

        /// <summary>
        /// Kind of advice
        /// </summary>
        public Kind Kind { get; }
    }
}