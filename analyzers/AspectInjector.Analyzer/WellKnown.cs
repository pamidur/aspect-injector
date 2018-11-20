using System;

namespace AspectInjector.Analyzer
{
    internal static class WellKnown
    {
        public static readonly Type AdviceType = typeof(Broker.Advice);
        public static readonly Type AdviceArgumentType = typeof(Broker.Advice.Argument);
        public static readonly Type MixinType = typeof(Broker.Mixin);
        public static readonly Type AspectType = typeof(Broker.Aspect);

        public static readonly Type Type = typeof(Type);
        public static readonly Type Object = typeof(Object);


        public static readonly string MixinTypeProperty = "mixin_type";
        public static readonly string AspectTypeProperty = "aspect_type";
    }
}
