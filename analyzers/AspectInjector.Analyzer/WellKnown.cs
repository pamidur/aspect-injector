using AspectInjector.Broker;
using System;
using System.Reflection;

namespace AspectInjector.Analyzer
{
    internal static class WellKnown
    {
        public static readonly string InjectionType = typeof(Injection).FullName;

        public static readonly string AdviceType = typeof(Advice).FullName;
        public static readonly string AdviceArgumentType = typeof(Argument).FullName;
        public static readonly string MixinType = typeof(Mixin).FullName;
        public static readonly string AspectType = typeof(Aspect).FullName;

        public static readonly string MethodBase = typeof(MethodBase).FullName;

        public static readonly string Type = typeof(Type).FullName;
        public static readonly string Attribute = typeof(Attribute).FullName;

        public static readonly string MixinTypeProperty = "mixin_type";
        public static readonly string AspectTypeProperty = "aspect_type";
    }
}
