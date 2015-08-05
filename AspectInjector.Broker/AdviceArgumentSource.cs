namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {
        Instance,
        Type,
        TargetName,
        TargetArguments,
        TargetValue,
        TargetException,
        AbortFlag,
        RoutableData
    }
}