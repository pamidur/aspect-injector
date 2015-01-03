using AspectInjector.Broker;
using AspectInjector.Tests.AdviceInjectionTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InjectionAdviceBeforeTests
    {
        private InjectionAdviceBeforeTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new InjectionAdviceBeforeTests_Target();
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

            var a = new InjectionAdviceBeforeTests_BeforeConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Aspect_With_Iface_Before_Constructor()
        {
            Checker.Passed = false;

            var a = new InjectionAdviceBeforeTests_BeforeConstructorWithIfaceTarget();
            Assert.IsTrue(Checker.Passed);
        }
    }

    //test classes

    [Aspect(typeof(InjectionAdviceBeforeTests_BeforeConstructorWithIfaceAspect))]
    internal class InjectionAdviceBeforeTests_BeforeConstructorWithIfaceTarget
    {
    }

    [Aspect(typeof(InjectionAdviceBeforeTests_BeforeConstructorAspect))]
    internal class InjectionAdviceBeforeTests_BeforeConstructorTarget
    {
        private StringBuider testField = new StringBuider();
    }

    [Aspect(typeof(InjectionAdviceBeforeTests_Aspect))]
    internal class InjectionAdviceBeforeTests_Target
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    //aspects

    internal class InjectionAdviceBeforeTests_Aspect
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

    internal class InjectionAdviceBeforeTests_BeforeConstructorAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Constructor)]
        public void BeforeConstructor([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
        {
            if (instance != null)
                Checker.Passed = true;
        }
    }

    [AdviceInterfaceProxy(typeof(IDisposable))]
    internal class InjectionAdviceBeforeTests_BeforeConstructorWithIfaceAspect : IDisposable
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Constructor)]
        public void BeforeConstructor() { Checker.Passed = true; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}