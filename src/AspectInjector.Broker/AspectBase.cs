using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspectInjector.Broker
{
    /// <summary>
    ///
    /// </summary>
    public abstract class AspectBase : Attribute
    {
        internal AspectBase()
        {
        }

        public string NameFilter { get; set; }
        public AccessModifier AccessFilter { get; set; }
        public MemberScope ScopeFilter { get; set; }

        [Flags]
        public enum AccessModifier
        {
            Any = 0,
            Private = 1,
            Protected = 2,
            Internal = 4,
            ProtectedInternal = 8,
            Public = 16
        }

        public enum MemberScope
        {
            /// <summary>
            /// Enumerates all members
            /// </summary>
            Auto = 0,

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