using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Proparation targets enumeration
    /// </summary>
    [Flags]
    public enum PropagateTo
    {
        /// <summary>
        /// Propagation is disabled
        /// </summary>
        None = 1,

        /// <summary>
        /// Propagate to methods
        /// </summary>
        Methods = 2,

        /// <summary>
        /// Propagate to properties
        /// </summary>
        Properties = 4,

        /// <summary>
        /// Propagate to events
        /// </summary>
        Events = 8,

        /// <summary>
        /// Propagate to all members
        /// </summary>
        Members = Methods + Properties + Events,

        /// <summary>
        /// Propagate to nested types. Causes recursive injection.
        /// </summary>
        Types = 16,

        /// <summary>
        /// Allows propagation to compiler generated members and types. Effectively allows propagation to lambdas and iterators. May affect performance. Use with caution!
        /// </summary>
        IncludeCompilerGenerated = 32,

        /// <summary>
        /// Propagate to everything. Could cause performance hit!
        /// </summary>
        Everything = Members + Types + IncludeCompilerGenerated
    }
}
