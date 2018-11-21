using System;

namespace AspectInjector.Analyzer
{
    internal static class WellKnown
    {
        public static readonly string AdviceType = typeof(Broker.Advice).FullName;
        public static readonly string AdviceArgumentType = typeof(Broker.Advice.Argument).FullName.Replace("+", ".");
        public static readonly string MixinType = typeof(Broker.Mixin).FullName;
        public static readonly string AspectType = typeof(Broker.Aspect).FullName;

        public static readonly string Type = typeof(Type).FullName;

        public static readonly string MixinTypeProperty = "mixin_type";
        public static readonly string AspectTypeProperty = "aspect_type";
    }
}
