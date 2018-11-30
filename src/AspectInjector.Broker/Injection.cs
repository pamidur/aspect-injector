using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Marks attribute as an injection trigger.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Injection : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Injection" /> class.
        /// </summary>
        /// <param name="aspect">Aspect to inject.</param>
        public Injection(Type aspect)
        {
        }

        /// <summary>
        /// Specifies priority for this cut. The higher priority the earlier execution.
        /// </summary>
        public ushort Priority { get; set; }

        ///// <summary>
        ///// Specifies subsequent members filter by name. The aspect will be applied to selected members as well. <code>null</code> means - all members.
        ///// </summary>
        //public string NameFilter { get; set; }

        ///// <summary>
        ///// Specifies subsequent members filter by access modifier. The aspect will be applied to selected members as well.
        ///// </summary>
        //public To ChildFilter { get; set; }
    }
}
