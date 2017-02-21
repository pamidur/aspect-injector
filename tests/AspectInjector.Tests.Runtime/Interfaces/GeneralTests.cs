using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace AspectInjector.Tests.Interfaces
{
    [TestClass]
    public class GeneralTests
    {
        private IGeneralTests _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = (IGeneralTests)new GeneralTests_Target();
        }

        [TestMethod]
        public void Interfaces_InjectAspectReference_SecondConstructor()
        {
            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            Checker.Passed = false;
            var result = ((IGeneralTests)new GeneralTests_Target(1)).TestMethod("ok", 1, ref ref1, out out1, ref ref2, out out2);
            Assert.AreEqual("ok", result);
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Interfaces_InjectMethodProxy()
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
        public void Interfaces_InjectSetterProxy()
        {
            Checker.Passed = false;
            _testClass.TestProperty = 4;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Interfaces_InjectGetterProxy()
        {
            Checker.Passed = false;
            var a = _testClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Interfaces_InjectEventAddProxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Interfaces_InjectEventRemoveProxy()
        {
            Checker.Passed = false;
            _testClass.TestEvent -= (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [Inject(typeof(GeneralTests_Aspect))]
        internal class GeneralTests_Target
        {
            public GeneralTests_Target()
            {
            }

            public GeneralTests_Target(int a)
            {
            }
        }

        internal interface IGeneralTests
        {
            string TestMethod(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue);

            event EventHandler<EventArgs> TestEvent;

            int TestProperty { get; set; }
        }

        /*[AdviceInterfaceProxy(typeof(INotifyPropertyChanged))]
        internal class INotifyPropertyChanged_Aspect : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
        }*/

        [Mixin(typeof(IGeneralTests))]
        [Mixin(typeof(INotifyPropertyChanged))]
        [Aspect(Aspect.Scope.Global)]
        internal class GeneralTests_Aspect : IGeneralTests, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

            string IGeneralTests.TestMethod(string data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue)
            {
                Checker.Passed = true;
                testOut = new object();
                testOutValue = 0;
                return data;
            }

            public event EventHandler<System.EventArgs> TestEvent
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