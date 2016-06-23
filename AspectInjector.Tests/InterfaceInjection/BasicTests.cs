using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace AspectInjector.Tests.InterfaceInjection
{
    [TestClass]
    public class BasicTests
    {
        private IInterfaceInjectionTests _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = (IInterfaceInjectionTests)new InterfaceInjectionTests_Target();
        }

        [TestMethod]
        public void Inject_AspectReference_Into_SecondConstructor()
        {
            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            Checker.Passed = false;
            var result = ((IInterfaceInjectionTests)new InterfaceInjectionTests_Target(1)).TestMethod("ok", 1, ref ref1, out out1, ref ref2, out out2);
            Assert.AreEqual("ok", result);
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Method_Proxy()
        {
            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            Checker.Passed = false;
            var result = _testClass.TestMethod("ok", 1, ref ref1, out out1, ref ref2, out out2);
            Assert.AreEqual("ok", result);
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Setter_Proxy()
        {
            Checker.Passed = false;
            _testClass.TestProperty = 4;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Getter_Proxy()
        {
            Checker.Passed = false;
            var a = _testClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_EventAdd_Proxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_EventRemove_Proxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent -= (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [Aspect(typeof(InterfaceInjectionTests_Aspect))]
        //[Aspect(typeof(INotifyPropertyChanged_Aspect))]
        internal class InterfaceInjectionTests_Target
        {
            public InterfaceInjectionTests_Target()
            {
            }

            public InterfaceInjectionTests_Target(int a)
            {
            }
        }

        internal interface IInterfaceInjectionTests
        {
            string TestMethod(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue);

            event EventHandler<EventArgs> TestEvent;

            int TestProperty { get; set; }
        }

        [AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
        internal class INotifyPropertyChanged_Aspect : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
        }

        [AdviceInterfaceProxy(typeof(IInterfaceInjectionTests))]
        internal class InterfaceInjectionTests_Aspect : IInterfaceInjectionTests
        {
            string IInterfaceInjectionTests.TestMethod(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue)
            {
                Checker.Passed = true;
                testOut = new object();
                testOutValue = 0;
                return data;
            }

            public event System.EventHandler<System.EventArgs> TestEvent
            {
                add { Checker.Passed = true; }
                remove { Checker.Passed = true; }
            }

            public int TestProperty
            {
                get { Checker.Passed = true; return 0; }
                set { Checker.Passed = true; }
            }
        }
    }
}