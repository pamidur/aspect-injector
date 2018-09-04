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
        public void Fact(string data)
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
}