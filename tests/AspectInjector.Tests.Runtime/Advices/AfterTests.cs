using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Advices
{
    public class AfterTests
    {
        private AfterTests_AfterMethodTarget _afterTestClass = new AfterTests_AfterMethodTarget();
      

        //after

        [Fact]
        public void Advices_InjectAfterMethod()
        {
            Checker.Passed = false;

            _afterTestClass.TestMethod("");
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestProperty = 1;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterCustomSetter()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 2;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterCustomSetter2()
        {
            Checker.Passed = false;

            _afterTestClass.TestCustomSetterProperty = 1;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterGetter()
        {
            Checker.Passed = false;

            var a = _afterTestClass.TestProperty;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterAddEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterRemoveEvent()
        {
            Checker.Passed = false;

            _afterTestClass.TestEvent += (s, e) => { };
            Assert.True(Checker.Passed);
        }

        //constructors

        [Fact]
        public void Advices_InjectAfterConstructor()
        {
            Checker.Passed = false;

            var a = new AfterTests_AfterConstructorTarget();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterSecondConstructor()
        {
            Checker.Passed = false;

            var a = new AfterTests_AfterConstructorTarget("");
            Assert.True(Checker.Passed);
        }
    }

    //test classes

    [AfterTests_AfterConstructorAspect]
    internal class AfterTests_AfterConstructorTarget
    {
        public AfterTests_AfterConstructorTarget()
        {
        }

        public AfterTests_AfterConstructorTarget(string a)
        {
        }
    }

    [AfterTests_AfterMethodAspect]
    internal class AfterTests_AfterMethodTarget
    {
        public void TestMethod(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }

        public int TestCustomSetterProperty
        {
            get
            {
                return 1;
            }
            set
            {
                {
                    if (value == 2) return;
                }
            }
        }
    }

    //aspects
    [Aspect(Scope.Global)]
    [Injection(typeof(AfterTests_AfterMethodAspect))]
    public class AfterTests_AfterMethodAspect : Attribute
    {
        //Property
        [Advice(Kind.After, Targets = Target.Setter)]
        public void AfterSetter() { Checker.Passed = true; }

        [Advice(Kind.After, Targets = Target.Getter)]
        public void AfterGetter() { Checker.Passed = true; }

        //Event
        [Advice(Kind.After, Targets = Target.EventAdd)]
        public void AfterEventAdd() { Checker.Passed = true; }

        [Advice(Kind.After, Targets = Target.EventRemove)]
        public void AfterEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(Kind.After, Targets = Target.Method)]
        public void AfterMethod() { Checker.Passed = true; }
    }

    [Aspect(Scope.Global)]
    [Injection(typeof(AfterTests_AfterConstructorAspect))]
    public class AfterTests_AfterConstructorAspect : Attribute
    {
        [Advice(Kind.After, Targets = Target.Constructor)]
        public void AfterConstructor() { Checker.Passed = true; }
    }   
}