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
            Checker.Passed = false;
            _beforeTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectBeforeSetter()
        {
            Checker.Passed = false;

            _beforeTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectBeforeGetter()
        {
            Checker.Passed = false;

            var a = _beforeTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectBeforeAddEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectBeforeRemoveEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);

        }

        //after

        [TestMethod]
        public void InjectAfterMethod()
        {
            Checker.Passed = false;

            _afterTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAftercustomSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 2;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAftercustomSetter2()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 1;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterGetter()
        {
            Checker.Passed = false;

            var a = _afterTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterAddEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterRemoveEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);

        }

        //constructors

        [TestMethod]
        public void InjectBeforeConstructor()
        {
            Checker.Passed = false;

            var a = new TestBeforeConstructor();
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterConstructor()
        {
            Checker.Passed = false;

            var a = new TestAfterConstructor();
            Assert.IsTrue(Checker.Passed);

        }

        [TestMethod]
        public void InjectAfterSecondConstructor()
        {
            Checker.Passed = false;

            var a = new TestAfterConstructor("");
            Assert.IsTrue(Checker.Passed);

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

        public int TestCustomSetterProperty
        {
            get { return 1; }
            set
            {
                {
                    if (value == 2) return;
                }
            }
        }
    }

    internal class TestAfterMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Setter)]
        public void AfterSetter() {  Checker.Passed = true; }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Getter)]
        public void AfterGetter() {  Checker.Passed = true; }

        //Event
        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.EventAdd)]
        public void AfterEventAdd() {  Checker.Passed = true; }

        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.EventRemove)]
        public void AfterEventRemove() {  Checker.Passed = true; }

        //Method
        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Method)]
        public void AfterMethod() {  Checker.Passed = true; }
    }

    internal class TestBeforeMethodAspect
    {
        //Property
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Setter)]
        public void BeforeSetter() {  Checker.Passed = true; }

        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Getter)]
        public void BeforeGetter() {  Checker.Passed = true; }

        //Event
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.EventAdd)]
        public void BeforeEventAdd() {  Checker.Passed = true; }

        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.EventRemove)]
        public void BeforeEventRemove() {  Checker.Passed = true; }

        //Method
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Method)]
        public void BeforeMethod() {  Checker.Passed = true; }
    }

    internal class TestBeforeConstructorAspect
    {
        [Advice(Points = InjectionPoints.Before, Targets = InjectionTargets.Constructor)]
        public void BeforeConstructor() {  Checker.Passed = true; }
    }

    internal class TestAfterConstructorAspect
    {
        [Advice(Points = InjectionPoints.After, Targets = InjectionTargets.Constructor)]
        public void AfterConstructor() {  Checker.Passed = true; }
    }
}