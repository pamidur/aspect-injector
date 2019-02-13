using AspectInjector.Broker;

namespace AspectInjector.Core
{
    public static class WellKnownTypes
    {
        public static readonly string Injection = typeof(Injection).FullName;
        public static readonly string Aspect = typeof(Aspect).FullName;
        public static readonly string Mixin = typeof(Mixin).FullName;
        public static readonly string Advice = typeof(Advice).FullName;
        public static readonly string Argument = typeof(Argument).FullName;
    }
}
