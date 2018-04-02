namespace AspectInjector.CLI
{
    interface ISwitch
    {
        string Description { get; }
        int Enable();
    }
}
