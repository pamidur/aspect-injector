using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Member access modifier enumeration.
    /// </summary>
    [Flags]
    public enum AccessModifier
    {
        /// <summary>
        /// All members.
        /// </summary>
        Any = 0,

        /// <summary>
        /// Private members.
        /// </summary>
        Private = 16,

        /// <summary>
        /// Internal members.
        /// </summary>
        Internal = 20,

        /// <summary>
        /// Protected members.
        /// </summary>
        Protected = 24,

        /// <summary>
        /// Protected internal members (Protected OR Internal).
        /// </summary>
        ProtectedInternal = 28,

        /// <summary>
        /// Protected AND internal members.
        /// </summary>
        PrivateProtected = 30,

        /// <summary>
        /// Public members.
        /// </summary>
        Public = 16,

        /// <summary>
        /// Static members.
        /// </summary>
        Static = 32,

        /// <summary>
        /// Non static members.
        /// </summary>
        NonStatic = 64
    }
}
