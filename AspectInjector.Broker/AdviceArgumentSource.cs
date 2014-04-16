namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {
        Instance,
        TargetName,
        TargetArguments,
        AbortFlag,
        ReturningValue,
        Exception,
        InjectionPointFired,
        CustomData
    }
}