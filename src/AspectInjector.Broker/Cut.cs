using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Marks member to be injection target for specific Aspect.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class Cut : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cut" /> class.
        /// </summary>
        /// <param name="aspect">Aspect to inject.</param>
        public Cut(Type aspect)
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
            Private = 1,

            /// <summary>
            /// Protected members.
            /// </summary>
            Protected = 2,

            /// <summary>
            /// Internal members.
            /// </summary>
            Internal = 4,

            /// <summary>
            /// Protected internal members.
            /// </summary>
            ProtectedInternal = 8,

            /// <summary>
            /// Public members.
            /// </summary>
            Public = 16
        }

        /// <summary>
        /// Member scope enumeration.
        /// </summary>
        public enum MemberScope
        {
            /// <summary>
            /// Enumerates all members.
            /// </summary>
            All = 0,

            /// <summary>
            /// Enumerates static members only.
            /// </summary>
            Type = 1,

            /// <summary>
            /// Enumerates instance members only.
            /// </summary>
            Instance = 2
        }
    }
}