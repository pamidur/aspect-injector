namespace AspectInjector.Test.Aspects
{
    [InterfaceProxyInjection(typeof(ITestInterface))]
    internal class TestInjectInterfaceAspect : ITestInterface
    {
        string ITestInterface.TestMethod(string data)
        {
            return "Aspect_" + data;
        }
    }
}