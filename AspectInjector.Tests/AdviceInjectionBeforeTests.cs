using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class AdviceInjectionBeforeTests
    {
        private AdviceInjectionBeforeTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new AdviceInjectionBeforeTests_Target();
        }

        [TestMethod]
        public void Inject_Aspect_Before_Method()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_Before_Setter()
        {
            Checker.Passed = false;

            _beforeTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_Before_Getter()
        {
            Checker.Passed = false;

            var a = _beforeTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_Before_AddEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_Before_RemoveEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_Before_Constructor()
        {
            Checker.Passed = false;

            var a = new AdviceInjectionBeforeTests_BeforeConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_With_Iface_Before_Constructor()
        {
            Checker.Passed = false;

            var a = new AdviceInjectionBeforeTests_BeforeConstructorWithIfaceTarget();
            Assert.IsTrue(Checker.Passed);
        }
    }

    //test classes

    [Aspect(typeof(AdviceInjectionBeforeTests_BeforeConstructorWithIfaceAspect))]
    internal class AdviceInjectionBeforeTests_BeforeConstructorWithIfaceTarget
    {
    }

    [Aspect(typeof(AdviceInjectionBeforeTests_BeforeConstructorAspect))]
    internal class AdviceInjectionBeforeTests_BeforeConstructorTarget
    {
        private AdviceInjectionBeforeTests_Target testField = new AdviceInjectionBeforeTests_Target();
    }

    [Aspect(typeof(AdviceInjectionBeforeTests_Aspect))]
    internal class AdviceInjectionBeforeTests_Target
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    //aspects

    internal class AdviceInjectionBeforeTests_Aspect
    {
        //Property
        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void BeforeSetter() { Checker.Passed = true; }

        [Advice(InjectionPoints.Before, InjectionTargets.Getter)]
        public void BeforeGetter() { Checker.Passed = true; }

        //Event
        [Advice(InjectionPoints.Before, InjectionTargets.EventAdd)]
        public void BeforeEventAdd() { Checker.Passed = true; }

        [Advice(InjectionPoints.Before, InjectionTargets.EventRemove)]
        public void BeforeEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    internal class AdviceInjectionBeforeTests_BeforeConstructorAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Constructor)]
        public void BeforeConstructor([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
        {
            if (instance != null)
                Checker.Passed = true;
        }
    }

    [AdviceInterfaceProxy(typeof(IDisposable))]
    internal class AdviceInjectionBeforeTests_BeforeConstructorWithIfaceAspect : IDisposable
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Constructor)]
        public void BeforeConstructor() { Checker.Passed = true; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}