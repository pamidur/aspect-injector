namespace AspectInjector.Test
{
    internal interface ITestInterface : ITestInterface2
    {
    }

    internal interface ITestInterface2
    {
        object TestMethod();
    }
}