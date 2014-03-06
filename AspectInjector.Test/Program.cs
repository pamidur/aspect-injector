using System;
using System.ComponentModel;

namespace AspectInjector.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestInterfaceInjection();
        }

        private static void TestInterfaceInjection()
        {
            var testInterface = (ITestInterface)new TestInterfaceProxyClass();

            testInterface.TestEvent += (s, e) => { };
            testInterface.TestEvent -= (s, e) => { };
            testInterface.TestMethod("");
            testInterface.TestProperty = 0;
            var a = testInterface.TestProperty;
            testInterface[0] = "";
            var b = testInterface[0];
        }
    }
}