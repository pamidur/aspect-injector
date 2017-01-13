using System;
using static AspectInjector.Broker.Cut;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Marks attribute class to be custom cut specification.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CutSpecification : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CutSpecification" /> class.
        /// </summary>
        /// <param name="aspect">Aspect to inject.</param>
        public CutSpecification(Type aspect)
        {
        }

        /// <summary>
        /// Specifies subsequent members filter by name. The aspect will be applied to selected members as well. <code>null</code> means - all members.
        /// </summary>
        public string NameFilter { get; set; }

        /// <summary>
        /// Specifies subsequent members filter by access modifier. The aspect will be applied to selected members as well.
        /// </summary>
        public AccessModifier AccessFilter { get; set; }

        /// <summary>
        /// Specifies subsequent members filter by scope. The aspect will be applied to selected members as well.
        /// </summary>
        public MemberScope ScopeFilter { get; set; }

        /// <summary>
        /// Specifies priority for this cut. The higher priority the earlier execution.
        /// </summary>
        public ushort Priority { get; set; }

        /// <summary>
        /// Specifies that this cut is a cut specification. Cut specification can mark only class that derived from Attribute.
        /// </summary>
    }
}