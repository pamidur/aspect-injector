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

    [Aspect(typeof(AfterTests_AfterConstructorAspect))]
    internal class AfterTests_AfterConstructorTarget
    {
        public AfterTests_AfterConstructorTarget()
        {
        }

        public AfterTests_AfterConstructorTarget(string a)
        {
        }
    }
    

    [Aspect(typeof(AfterTests_AfterMethodAspect))]
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
        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterSetter() { Checker.Passed = true; }

        [Advice(InjectionPoints.After, InjectionTargets.Getter)]
        public void AfterGetter() { Checker.Passed = true; }

        //Event
        [Advice(InjectionPoints.After, InjectionTargets.EventAdd)]
        public void AfterEventAdd() { Checker.Passed = true; }

        [Advice(InjectionPoints.After, InjectionTargets.EventRemove)]
        public void AfterEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod() { Checker.Passed = true; }
    }       

    internal class AfterTests_AfterConstructorAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void AfterConstructor() { Checker.Passed = true; }
    }

    internal class AfterTests_SetterValueTarget
    {
        public static int GlobalData = 0;

        [Aspect(typeof(AfterTests_SetterValueAspect))]
        public int Data { get; set; }
    }

    internal class AfterTests_SetterValueAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.ReturnValue)] object old)
        {
            Checker.Passed = (int)old == AfterTests_SetterValueTarget.GlobalData;
        }
    }
}