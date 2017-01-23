using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Marks member to be injection target for specific Aspect.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class Inject : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Inject" /> class.
        /// </summary>
        /// <param name="aspect">Aspect to inject.</param>
        public Inject(Type aspect)
        {
        }

        ///// <summary>
        ///// Specifies subsequent members filter by name. The aspect will be applied to selected members as well. <code>null</code> means - all members.
        ///// </summary>
        //public string NameFilter { get; set; }

        ///// <summary>
        ///// Specifies subsequent members filter by access modifier. The aspect will be applied to selected members as well.
        ///// </summary>
        //public To ChildFilter { get; set; }

        /// <summary>
        /// Specifies priority for this cut. The higher priority the earlier execution.
        /// </summary>
        public ushort Priority { get; set; }

        ///// <summary>
        ///// Member access modifier enumeration.
        ///// </summary>

        //Everything = 0,
        //Private = 16,
        //Internal = 20,
        //Protected = 24,
        //ProtectedOrInternal = 28,
        //ProtectedAndInternal = 30,
        //Public = 31,

        ////AllAccess = 0x00011111,

        //NonStatic = 32,
        //Static = 48,

        //[Flags]
        //public enum To
        //{
        //    /// <summary>
        //    /// All members.
        //    /// </summary>
        //    Everything = 0,

        //    /// <summary>
        //    /// Private members.
        //    /// </summary>
        //    Private = 16,

        //    /// <summary>
        //    /// Internal members.
        //    /// </summary>
        //    Internal = 20,

        //    /// <summary>
        //    /// Protected members.
        //    /// </summary>
        //    Protected = 24,

        //    /// <summary>
        //    /// Protected internal members (Protected OR Internal).
        //    /// </summary>
        //    ProtectedInternal = 28,

        //    [EditorBrowsable(EditorBrowsableState.Never)]
        //    /// <summary>
        //    /// Protected AND internal members.
        //    /// </summary>
        //    ProtectedAndInternal = 30,

        //    /// <summary>
        //    /// Public members.
        //    /// </summary>
        //    Public = 16,

        //    /// <summary>
        //    /// Static members.
        //    /// </summary>
        //    Static = 32,

        //    /// <summary>
        //    /// Non static members.
        //    /// </summary>
        //    NonStatic = 64
        //}
    }
}