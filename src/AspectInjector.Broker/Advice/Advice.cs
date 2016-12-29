using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Defines method as a special advice method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class Advice : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Advice" /> class.
        /// </summary>
        /// <param name="type">Specifies when advice method should be called.</param>
        /// <param name="targets">Specifies what target members this method is injected to.</param>
        public Advice(Type type, Target targets)
        {
        }

        /// <summary>
        /// Advice method injection points enumeration.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Advice method is called before target method.
            /// </summary>
            Before = 1,

            /// <summary>
            /// Advice method is called after target method.
            /// </summary>
            After = 2,

            /// <summary>
            /// Advice method is called instead of target method. Consider using <see cref="AdviceArgument.Source.Target" /> and <see cref="AdviceArgument.Source.Arguments" /> in order to make a subsequent call to target method.
            /// </summary>
            Around = 4
        }

        /// <summary>
        /// Advice method injection targets enumeration.
        /// </summary>
        [Flags]
        public enum Target
        {
            /// <summary>
            /// Advice method is injected into target's constructors.
            /// </summary>
            Constructor = 1,

            /// <summary>
            /// Advice method is injected into target's methods.
            /// </summary>
            Method = 2,

            /// <summary>
            /// Advice method is injected into target's property getters.
            /// </summary>
            Getter = 4,

            /// <summary>
            /// Advice method is injected into target's property setters.
            /// </summary>
            Setter = 8,

            /// <summary>
            /// Advice method is injected into target's event subscribe handlers.
            /// </summary>
            EventAdd = 16,

            /// <summary>
            /// Advice method is injected into target's event unsubscribe handlers.
            /// </summary>
            EventRemove = 32
        }
    }
}