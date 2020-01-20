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
            Aspect = aspect;
        }

        /// <summary>
        /// Specifies priority for this cut. The higher priority the earlier execution.
        /// </summary>
        public ushort Priority { get; set; } = 0;
        /// <summary>
        /// Aspect type that is being injected.
        /// </summary>
        public Type Aspect { get; } = null;

        /// <summary>
        /// Specifies propagation strategy. <see cref="PropagateTo.Members" /> | <see cref="PropagateTo.Types" /> by default.
        /// </summary>
        public PropagateTo Propagation { get; set; } = PropagateTo.Members | PropagateTo.Types;

        /// <summary>
        /// Specifies subsequent members filter by name. The aspect will be applied to selected members as well. <code>null</code> means - all members.
        /// </summary>
        public string PropagationFilter { get; set; } = null;

        /// <summary>
        /// Gets or sets a System.Boolean value that determines whether the indicated injection is inherited by derived attribute classes.
        /// </summary>
        public bool Inherited { get; set; } = false;
    }
}
