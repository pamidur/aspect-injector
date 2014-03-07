using AspectInjector.Broker;
using System;

namespace AspectInjector.Test
{
    [Aspect(typeof(TestInterfaceAspect))]
    internal class TestInterfaceProxyClass
    {
    }

    internal interface ITestInterface
    {
        string TestMethod(string data);

        event EventHandler<EventArgs> TestEvent;

        int TestProperty { get; set; }
    }

    [AdviceInterfaceProxy(typeof(ITestInterface))]
    internal class TestInterfaceAspect : ITestInterface
    {
        string ITestInterface.TestMethod(string data)
        {
            Console.WriteLine("TestMethod"); return "";
        }

        public event System.EventHandler<System.EventArgs> TestEvent
        {
            add { Console.WriteLine("TestEvent_add"); }
            remove { Console.WriteLine("TestEvent_remove"); }
        }

        public int TestProperty
        {
            get { Console.WriteLine("TestProperty_get"); return 0; }
            set { Console.WriteLine("TestProperty_set"); }
        }
    }
}