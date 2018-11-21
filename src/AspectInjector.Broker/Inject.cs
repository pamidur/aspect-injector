using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Marks member to be injection target for specific Aspect.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
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

        ///// <summary>
        ///// Specifies priority for this cut. The higher priority the earlier execution.
        ///// </summary>
        //public ushort Priority { get; set; }
    }
}