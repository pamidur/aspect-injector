using AspectInjector.Broker;
using Xunit;
using System;

namespace AspectInjector.Tests.Advices
{
    public class BeforeTests
    {
        private BeforeTests_Target _beforeTestClass = new BeforeTests_Target();

        [Fact]
        public void Advices_InjectBeforeMethod()
        {
            Checker.Passed = false;
            _beforeTestClass.Fact("");
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeSetter()
        {
            Checker.Passed = false;

            _beforeTestClass.TestProperty = 1;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeGetter()
        {
            Checker.Passed = false;

            var a = _beforeTestClass.TestProperty;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeAddEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeRemoveEvent()
        {
            Checker.Passed = false;

            _beforeTestClass.TestEvent += (s, e) => { };
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeConstructor()
        {
            Checker.Passed = false;

            var a = new BeforeTests_BeforeConstructorTarget();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeConstructor_WithInterface()
        {
            Checker.Passed = false;

            var a = new BeforeTests_BeforeConstructorWithInterfaceTarget();
            Assert.True(Checker.Passed);
        }
    }

    //test classes

    [BeforeTests_BeforeConstructorWithInterfaceAspect]
    internal class BeforeTests_BeforeConstructorWithInterfaceTarget
    {
    }

    [BeforeTests_BeforeConstructorAspect]
    internal class BeforeTests_BeforeConstructorTarget
    {
        private BeforeTests_Target testField = new BeforeTests_Target();
    }

    [BeforeTests_Aspect]
    internal class BeforeTests_Target
    {
        public void Fact(string data)
        {
        }

        public event EventHandler<EventArgs> TestEvent = (s, e) => { };

        public int TestProperty { get; set; }
    }

    //aspects
    [Aspect(Scope.Global)]
    [Injection(typeof(BeforeTests_Aspect))]
    public class BeforeTests_Aspect : Attribute
    {
        //Property
        [Advice(Kind.Before, Targets = Target.Setter)]
        public void BeforeSetter() { Checker.Passed = true; }

        [Advice(Kind.Before, Targets = Target.Getter)]
        public void BeforeGetter() { Checker.Passed = true; }

        //Event
        [Advice(Kind.Before, Targets = Target.EventAdd)]
        public void BeforeEventAdd() { Checker.Passed = true; }

        [Advice(Kind.Before, Targets = Target.EventRemove)]
        public void BeforeEventRemove() { Checker.Passed = true; }

        //Method
        [Advice(Kind.Before, Targets = Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    [Aspect( Scope.Global)]
    [Injection(typeof(BeforeTests_BeforeConstructorAspect))]
    public class BeforeTests_BeforeConstructorAspect : Attribute
    {
        [Advice(Kind.Before, Targets = Target.Constructor)]
        public void BeforeConstructor([Argument(Source.Instance)] object instance)
        {
            if (instance != null)
                Checker.Passed = true;
        }
    }

    [Mixin(typeof(IDisposable))]
    [Aspect(Scope.Global)]
    [Injection(typeof(BeforeTests_BeforeConstructorWithInterfaceAspect))]
    public class BeforeTests_BeforeConstructorWithInterfaceAspect : Attribute, IDisposable
    {
        [Advice(Kind.Before, Targets = Target.Constructor)]
        public void BeforeConstructor() { Checker.Passed = true; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}