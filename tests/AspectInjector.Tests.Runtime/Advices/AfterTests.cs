using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class AfterTests
    {
        private AfterTests_AfterMethodTarget _afterTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _afterTestClass = new AfterTests_AfterMethodTarget();
        }

        //after

        [TestMethod]
        public void Advices_InjectAfterMethod()
        {
            Checker.Passed = false;

            _afterTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterCustomSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 2;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterCustomSetter2()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterGetter()
        {
            Checker.Passed = false;

            var a = _afterTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterAddEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterRemoveEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        //constructors

        [TestMethod]
        public void Advices_InjectAfterConstructor()
        {
            Checker.Passed = false;

            var a = new AfterTests_AfterConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterSecondConstructor()
        {
            Checker.Passed = false;

            var a = new AfterTests_AfterConstructorTarget("");
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterSetter_AccessOldValue()
        {
            Checker.Passed = false;

            var a = new AfterTests_SetterValueTarget();
            a.Data = 2;
            AfterTests_SetterValueTarget.GlobalData = 2;
            a.Data = 4;

            Assert.IsTrue(Checker.Passed);
        }
    }

    //test classes

    [Cut(typeof(AfterTests_AfterConstructorAspect))]
    internal class AfterTests_AfterConstructorTarget
    {
        public AfterTests_AfterConstructorTarget()
        {
        }

        public AfterTests_AfterConstructorTarget(string a)
        {
        }
    }

    [Cut(typeof(AfterTests_AfterMethodAspect))]
    internal class AfterTests_AfterMethodTarget
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

    //aspects

    internal class AfterTests_AfterMethodAspect
    {
        //Property
        [Advice(Advice.Type.After, Advice.Target.Setter)]
        public void AfterSetter() { Checker.Passed = true; }

        [Advice(Advice.Type.After, Advice.Target.Getter)]
        public void AfterGetter() { Checker.Passed = true; }

        //Event
        [Advice(Advice.Type.After, Advice.Target.EventAdd)]
        public void AfterEventAdd() { Checker.Passed = true; }

        [Advice(Advice.Type.After, Advice.Target.EventRemove)]
        public void AfterEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(Advice.Type.After, Advice.Target.Method)]
        public void AfterMethod() { Checker.Passed = true; }
    }

    internal class AfterTests_AfterConstructorAspect
    {
        [Advice(Advice.Type.After, Advice.Target.Constructor)]
        public void AfterConstructor() { Checker.Passed = true; }
    }

    internal class AfterTests_SetterValueTarget
    {
        public static int GlobalData = 0;

        [Cut(typeof(AfterTests_SetterValueAspect))]
        public int Data { get; set; }
    }

    internal class AfterTests_SetterValueAspect
    {
        [Advice(Advice.Type.After, Advice.Target.Setter)]
        public void AfterMethod([AdviceArgument(AdviceArgument.Source.ReturnValue)] object old)
        {
            Checker.Passed = (int)old == AfterTests_SetterValueTarget.GlobalData;
        }
    }
}