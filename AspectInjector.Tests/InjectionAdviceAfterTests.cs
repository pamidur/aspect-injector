using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InjectionAdviceAfterTests
    {        
        private InjectionAdviceAfterTests_Target _afterTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _afterTestClass = new InjectionAdviceAfterTests_Target();
        }        

        //after

        [TestMethod]
        public void Inject_Advice_After_Method()
        {
            Checker.Passed = false;

            _afterTestClass.TestMethod("");
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_Setter()
        {
            Checker.Passed = false;

            _afterTestClass.TestProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_Custom_Setter()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 2;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_Custom_Setter2()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 1;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_Getter()
        {
            Checker.Passed = false;

            var a = _afterTestClass.TestProperty;
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_AddEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_RemoveEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.IsTrue(Checker.Passed);
        }

        //constructors          

        [TestMethod]
        public void Inject_Advice_After_Constructor()
        {
            Checker.Passed = false;

            var a = new InjectionAdviceAfterTests_AfterConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_After_SecondConstructor()
        {
            Checker.Passed = false;

            var a = new InjectionAdviceAfterTests_AfterConstructorTarget("");
            Assert.IsTrue(Checker.Passed);
        }
    }    
        
    //test classes

    [Aspect(typeof(InjectionAdviceAfterTests_AfterConstructorAspect))]
    internal class InjectionAdviceAfterTests_AfterConstructorTarget
    {
        public InjectionAdviceAfterTests_AfterConstructorTarget()
        {
        }

        public InjectionAdviceAfterTests_AfterConstructorTarget(string a)
        {
        }
    }
    

    [Aspect(typeof(InjectionAdviceAfterTests_AfterMethodAspect))]
    internal class InjectionAdviceAfterTests_Target
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

    internal class InjectionAdviceAfterTests_AfterMethodAspect
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

    internal class InjectionAdviceAfterTests_AfterConstructorAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void AfterConstructor() { Checker.Passed = true; }
    }    
}