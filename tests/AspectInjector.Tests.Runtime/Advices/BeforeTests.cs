using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class BeforeTests
    {
        private BeforeTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new BeforeTests_Target();
        }

        [TestMethod]
        public void Advices_InjectBeforeMethod()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeSetter()
        {
            Checker.Passed = false;

            _beforeTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeGetter()
        {
            Checker.Passed = false;

            var a = _beforeTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeAddEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeRemoveEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeConstructor()
        {
            Checker.Passed = false;

            var a = new BeforeTests_BeforeConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeConstructor_WithInterface()
        {
            Checker.Passed = false;

            var a = new BeforeTests_BeforeConstructorWithInterfaceTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeSetter_AccessOldValue()
        {
            Checker.Passed = false;

            var a = new BeforeTests_SetterValueTarget();
            a.Data = 2;
            BeforeTests_SetterValueTarget.GlobalData = 2;
            a.Data = 4;

            Assert.IsTrue(Checker.Passed);
        }
    }

    //test classes

    [Inject(typeof(BeforeTests_BeforeConstructorWithInterfaceAspect))]
    internal class BeforeTests_BeforeConstructorWithInterfaceTarget
    {
    }

    [Inject(typeof(BeforeTests_BeforeConstructorAspect))]
    internal class BeforeTests_BeforeConstructorTarget
    {
        private BeforeTests_Target testField = new BeforeTests_Target();
    }

    [Inject(typeof(BeforeTests_Aspect))]
    internal class BeforeTests_Target
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    //aspects
    [Aspect(Aspect.Scope.Global)]
    internal class BeforeTests_Aspect
    {
        //Property
        [Advice(Advice.Type.Before, Advice.Target.Setter)]
        public void BeforeSetter() { Checker.Passed = true; }

        [Advice(Advice.Type.Before, Advice.Target.Getter)]
        public void BeforeGetter() { Checker.Passed = true; }

        //Event
        [Advice(Advice.Type.Before, Advice.Target.EventAdd)]
        public void BeforeEventAdd() { Checker.Passed = true; }

        [Advice(Advice.Type.Before, Advice.Target.EventRemove)]
        public void BeforeEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class BeforeTests_BeforeConstructorAspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Constructor)]
        public void BeforeConstructor([Advice.Argument(Advice.Argument.Source.Instance)] object instance)
        {
            if (instance != null)
                Checker.Passed = true;
        }
    }

    [Mixin(typeof(IDisposable))]
    [Aspect(Aspect.Scope.Global)]
    internal class BeforeTests_BeforeConstructorWithInterfaceAspect : IDisposable
    {
        [Advice(Advice.Type.Before, Advice.Target.Constructor)]
        public void BeforeConstructor() { Checker.Passed = true; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    internal class BeforeTests_SetterValueTarget
    {
        public static int GlobalData = 0;

        [Inject(typeof(BeforeTests_SetterValueAspect))]
        public int Data { get; set; }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class BeforeTests_SetterValueAspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Setter)]
        public void AfterMethod(
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object old
            )
        {
            Checker.Passed = (int)old == BeforeTests_SetterValueTarget.GlobalData;
        }
    }
}