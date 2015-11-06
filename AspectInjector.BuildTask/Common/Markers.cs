namespace AspectInjector.BuildTask.Common
{
    public static class Markers
    {
        public static readonly object InstanceSelfMarker = new object();
        public static readonly object DefaultMarker = new object();
        public static readonly object TargetFuncMarker = new object();
    }
}