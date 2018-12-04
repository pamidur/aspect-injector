using AspectInjector.Broker;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core
{
    public static class WellKnownTypes
    {
        public static readonly string Type = typeof(System.Type).FullName;
        public static readonly string Object = typeof(object).FullName;
        public static readonly string Void = typeof(void).FullName;

        public static readonly string IteratorStateMachineAttribute = typeof(IteratorStateMachineAttribute).FullName;
        public static readonly string AsyncStateMachineAttribute = typeof(AsyncStateMachineAttribute).FullName;

        public static readonly string Injection = typeof(Injection).FullName;
        public static readonly string Aspect = typeof(Aspect).FullName;
        public static readonly string Mixin = typeof(Mixin).FullName;
        public static readonly string Advice = typeof(Advice).FullName;
        public static readonly string Argument = typeof(Argument).FullName;
    }
}
