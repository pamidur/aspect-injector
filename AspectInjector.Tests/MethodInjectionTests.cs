using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class MethodInjectionTests
    {
        private TestBeforeMethodClass _beforeTestClass;
        private TestAfterMethodClass _afterTestClass;

        //before
        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new TestBeforeMethodClass();
            _afterTestClass = new TestAfterMethodClass();
        }

        [TestMethod]
        public void InjectBeforeMethod()
        {
            _beforeTestClass.TestMethod("");
        }

        [TestMethod]
        public void InjectBeforeSetter()
        {
            _beforeTestClass.TestProperty = 1;
        }

        [TestMethod]
        public void InjectBeforeGetter()
        {
            var a = _beforeTestClass.TestProperty;
        }

        [TestMethod]
        public void InjectBeforeAddEvent()
        {
            _beforeTestClass.TestEvent += (s, e) => { };
        }

        [TestMethod]
        public void InjectBeforeRemoveEvent()
        {
            _beforeTestClass.TestEvent += (s, e) => { };
        }

        //after

        [TestMethod]
        public void InjectAfterMethod()
        {
            _afterTestClass.TestMethod("");
        }

        [TestMethod]
        public void InjectAfterSetter()
        {
            _afterTestClass.TestProperty = 1;
        }

        [TestMethod]
        public void InjectAfterGetter()
        {
            var a = _afterTestClass.TestProperty;
        }

        [TestMethod]
        public void InjectAfterAddEvent()
        {
            _afterTestClass.TestEvent += (s, e) => { };
        }

        [TestMethod]
        public void InjectAfterRemoveEvent()
        {
            _afterTestClass.TestEvent += (s, e) => { };
        }

        //constructors

        [TestMethod]
        public void InjectBeforeConstructor()
        {
            var a = new TestBeforeConstructor();
        }

        [TestMethod]
        public void InjectAfterConstructor()
        {
            var a = new TestAfterConstructor();
        }

        [TestMethod]
        public void InjectAfterSecondConstructor()
        {
            var a = new TestAfterConstructor("");
        }
    }

    [Aspect(typeof(TestBeforeConstructorAspect))]
    internal class TestBeforeConstructor
    {
    }

    [Aspect(typeof(TestAfterConstructorAspect))]
    internal class TestAfterConstructor
    {
        public TestAfterConstructor()
        {
        }

        public TestAfterConstructor(string a)
        {
        }
    }

    [Aspect(typeof(TestBeforeMethodAspect))]
    internal class TestBeforeMethodClass
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    [Aspect(typeof(TestAfterMethodAspect))]
    internal class TestAfterMethodClass
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    internal class TestAfterMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Setter)]
        public void AfterSetter() { Assert.IsTrue(true); }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Getter)]
        public void AfterGetter() { Assert.IsTrue(true); }

        //Event
        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.EventAdd)]
        public void AfterEventAdd() { Assert.IsTrue(true); }

        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.EventRemove)]
        public void AfterEventRemove() { Assert.IsTrue(true); }

        //Method
        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Method)]
        public void AfterMethod() { Assert.IsTrue(true); }
    }

    internal class TestBeforeMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Setter)]
        public void BeforeSetter() { Assert.IsTrue(true); }

        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Getter)]
        public void BeforeGetter() { Assert.IsTrue(true); }

        //Event
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.EventAdd)]
        public void BeforeEventAdd() { Assert.IsTrue(true); }

        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.EventRemove)]
        public void BeforeEventRemove() { Assert.IsTrue(true); }

        //Method
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Method)]
        public void BeforeMethod() { Assert.IsTrue(true); }
    }

    internal class TestBeforeConstructorAspect
    {
        [Advice(Points = InjectionPoint.Before, Targets = InjectionTarget.Constructor)]
        public void BeforeConstructor() { Assert.IsTrue(true); }
    }

    internal class TestAfterConstructorAspect
    {
        [Advice(Points = InjectionPoint.After, Targets = InjectionTarget.Constructor)]
        public void AfterConstructor() { Assert.IsTrue(true); }
    }
}