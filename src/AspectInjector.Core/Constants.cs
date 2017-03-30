namespace AspectInjector.Core
{
    public static class Constants
    {
        public static readonly string Prefix = "__a$_";
        public static readonly string AssetsResourceName = $"{Prefix}assets";
        public static readonly string AspectGlobalField = $"{Prefix}instance";
        public static readonly string AspectInstanceFieldPrefix = $"{Prefix}a_";
        public static readonly string InstanceAspectsMethodName = $"{Prefix}initialize_aspects";

        public static readonly string AspectFactoryMethodName = "GetInstance";
    }
}