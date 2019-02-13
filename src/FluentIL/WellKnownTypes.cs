using System.Runtime.CompilerServices;

namespace FluentIL
{
    public static class WellKnownTypes
    {
        public static readonly string Type = typeof(System.Type).FullName;
        public static readonly string Object = typeof(object).FullName;
        public static readonly string Void = typeof(void).FullName;

        public static readonly string IteratorStateMachineAttribute = typeof(IteratorStateMachineAttribute).FullName;
        public static readonly string AsyncStateMachineAttribute = typeof(AsyncStateMachineAttribute).FullName;
    }
}
