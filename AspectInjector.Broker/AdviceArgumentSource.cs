namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {
        Instance,
        Type,
        Method,

        Target,
        TargetReturnType,

        Name,
        Arguments,
        ReturnValue,

        RoutableData
    }
}