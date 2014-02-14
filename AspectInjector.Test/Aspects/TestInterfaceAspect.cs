namespace AspectInjector.Test.Aspects
{
    [AdviceInterfaceProxy(typeof(ITestInterface))]
    internal class TestInterfaceAspect : ITestInterface
    {
        string ITestInterface.TestMethod(string data)
        {
            return "Aspect_" + data;
        }
    }
}