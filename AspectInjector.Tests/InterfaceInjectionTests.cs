using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InterfaceInjectionTests
    {
        private ITestInterface _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = (ITestInterface)new TestInterfaceProxyClass();
        }

        [TestMethod]
        public void InjectProxySecondConstructor()
        {
            var result = ((ITestInterface)new TestInterfaceProxyClass(1)).TestMethod("ok");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void InjectMethodProxy()
        {
            var result = _testClass.TestMethod("ok");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void InjectSetterProxy()
        {
            _testClass.TestProperty = 4;
        }

        [TestMethod]
        public void InjectGetterProxy()
        {
            var a = _testClass.TestProperty;
        }

        [TestMethod]
        public void InjectEventAddProxy()
        {
            _testClass.TestEvent += (s, e) => { };
        }

        [TestMethod]
        public void InjectEventRemoveProxy()
        {
            _testClass.TestEvent -= (s, e) => { };
        }
    }

    [Aspect(typeof(TestInterfaceAspect))]
    internal class TestInterfaceProxyClass
    {
        public TestInterfaceProxyClass()
        {
        }

        public TestInterfaceProxyClass(int a)
        {
        }
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
            Assert.IsTrue(true);
            return data;
        }

        public event System.EventHandler<System.EventArgs> TestEvent
        {
            add { Assert.IsTrue(true); }
            remove { Assert.IsTrue(true); }
        }

        public int TestProperty
        {
            get { Assert.IsTrue(true); return 0; }
            set { Assert.IsTrue(true); ; }
        }
    }
}