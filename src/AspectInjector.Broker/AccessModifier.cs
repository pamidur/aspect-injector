using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Member access modifier enumeration.
    /// </summary>
    [Flags]
    public enum AccessModifier : byte
    {
        /// <summary>
        /// All members.
        /// </summary>
        Any = 0,

        /// <summary>
        /// Private members.
        /// </summary>
        Private = 1,

        /// <summary>
        /// Internal members.
        /// </summary>
        Internal = 2,

        /// <summary>
        /// Protected members.
        /// </summary>
        Protected = 4,

        /// <summary>
        /// Protected internal members (Protected OR Internal).
        /// </summary>
        ProtectedInternal = 8,

        /// <summary>
        /// Protected AND internal members.
        /// </summary>
        ProtectedPrivate = 16,

        /// <summary>
        /// Public members.
        /// </summary>
        Public = 32,

        /// <summary>
        /// Static members.
        /// </summary>
        Static = 64,

        /// <summary>
        /// Non static members.
        /// </summary>
        Instance = 128
    }
}
