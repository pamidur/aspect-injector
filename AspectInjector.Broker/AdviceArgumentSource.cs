namespace AspectInjector.Broker
{
    public enum AdviceArgumentSource
    {
        Instance,
        TargetName,
        TargetArguments,
        TargetReturnValue,
        TargetException,
        AbortFlag,
        CustomData
    }
}