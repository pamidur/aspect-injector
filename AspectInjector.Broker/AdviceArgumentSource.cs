namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {   
        Instance,
        TargetName,
        TargetArguments,
        TargetValue,
        TargetException,
        AbortFlag,
        RoutableData
    }
}