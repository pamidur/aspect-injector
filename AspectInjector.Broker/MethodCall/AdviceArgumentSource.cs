namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {
        Instance,
        Type,
        Method,

        Target,

        Name,
        Arguments,
        ReturnValue,
        ReturnType,

        RoutableData
    }
}