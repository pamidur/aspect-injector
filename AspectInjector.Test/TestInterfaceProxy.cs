using AspectInjector.Broker;
using System;
using System.Linq.Expressions;

namespace AspectInjector.Test
{
    internal class Model
    {
        public int Data { get; set; }
    }

    internal class ViewModel
    {
        public Model Model { get; set; }

        public int Data
        {
            get { return Model.Data; }
            set { Model.Data = value; }
        }

        private Expression<Func<string>> a = () => "ololo";
    }

    internal class check : ITestInterface
    {
        private int TestProperty { get; set; }

        string ITestInterface.TestMethod(string data)
        {
            throw new NotImplementedException();
        }

        int ITestInterface.TestProperty
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<EventArgs> ITestInterface.TestEvent
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
    }

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