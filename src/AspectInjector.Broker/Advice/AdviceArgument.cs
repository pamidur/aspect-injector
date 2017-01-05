using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Defines method parameter as a special advice parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class AdviceArgument : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceArgument" /> class.
        /// </summary>
        /// <param name="source">Specifies source of advice argument.</param>
        public AdviceArgument(Source source)
        {
        }

        /// <summary>
        /// Advice argument sources enumeration.
        /// </summary>
        public enum Source
        {
            /// <summary>
            /// Target's instance or <c>null</c> if target is static.
            /// Should be of type <see cref="object" />.
            /// </summary>
            Instance,

            /// <summary>
            /// Target's class type.
            /// Should be of type <see cref="System.Type" />.
            /// </summary>
            Type,

            /// <summary>
            /// Target method.
            /// Should be of type <see cref="System.Reflection.MethodBase" />.
            /// </summary>
            Method,

            /// <summary>
            /// Target method delegate. Usage <example>Target(<see cref="Arguments"/>)</example> for chaining methods.
            /// Should be of type <c>Func&lt;object[],object&gt;</c>.
            /// Works only with <see cref="Advice.Type.Around" />.
            /// </summary>
            Target,

            /// <summary>
            /// Target name.
            /// Should be of type <see cref="string" />.
            /// </summary>
            Name,

            /// <summary>
            /// Target method arguments.
            /// Should be of type <c>object[]</c>.
            /// </summary>
            Arguments,

            /// <summary>
            /// Target method result.
            /// Should be of type <see cref="object" />.
            /// </summary>
            ReturnValue,

            /// <summary>
            /// Target method result type.
            /// Should be of type <see cref="System.Type" />.
            /// </summary>
            ReturnType,

            /// <summary>
            /// Set of Attributes that defines <see cref="Cut.IsSpec" />.
            /// Should be of type <c>Attribute[]</c>.
            /// </summary>
            RoutableData
        }
    }
}