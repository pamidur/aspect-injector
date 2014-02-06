using System;

namespace AspectInjector.Test.Aspects
{
    [InterfaceProxyInjection(typeof(ITestInterface))]
    internal class TestInjectInterfaceAspect : TestInjectInterfaceAspect2
    {
        //public override object TestMethod()
        //{
        //    Console.WriteLine("TestInjectInterfaceAspect.TestMethod");
        //    return "";
        //}
    }

    internal class TestInjectInterfaceAspect2 : ITestInterface
    {
        //public virtual object TestMethod()
        //{
        //    Console.WriteLine("TestInjectInterfaceAspect2.TestMethod");
        //    return "";
        //}

        object ITestInterface2.TestMethod()
        {
            throw new NotImplementedException();
        }

        private object TestMethod()
        {
            throw new NotImplementedException();
        }
    }
}