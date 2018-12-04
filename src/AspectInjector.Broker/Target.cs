using System;

namespace AspectInjector.Broker
{
    /// <summary>
    /// Advice method injection targets enumeration.
    /// </summary>
    [Flags]
    public enum Target : ushort
    {
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
        /// Members of any access.
        /// </summary>
        AnyAccess = Private + Internal + Protected + ProtectedInternal + ProtectedPrivate + Public,


        /// <summary>
        /// Static members.
        /// </summary>
        Static = 64,

        /// <summary>
        /// Non static members.
        /// </summary>
        Instance = 128,

        /// <summary>
        /// Members of any scope.
        /// </summary>
        AnyScope = Instance + Static,


        /// <summary>
        /// Advice method is injected into target's constructors.
        /// </summary>
        Constructor = 256,

        /// <summary>
        /// Advice method is injected into target's methods.
        /// </summary>
        Method = 512,

        /// <summary>
        /// Advice method is injected into target's property getters.
        /// </summary>
        Getter = 1024,

        /// <summary>
        /// Advice method is injected into target's property setters.
        /// </summary>
        Setter = 2048,

        /// <summary>
        /// Advice method is injected into target's event subscribe handlers.
        /// </summary>
        EventAdd = 4096,

        /// <summary>
        /// Advice method is injected into target's event unsubscribe handlers.
        /// </summary>
        EventRemove = 8192,

        /// <summary>
        /// Advice method is injected into any target's member.
        /// </summary>
        AnyMember = Constructor + Method + Getter + Setter + EventAdd + EventRemove,


        /// <summary>
        /// All members.
        /// </summary>
        Any = AnyAccess + AnyScope + AnyMember
    }
}
