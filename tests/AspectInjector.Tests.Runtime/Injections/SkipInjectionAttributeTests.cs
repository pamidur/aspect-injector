using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Injections
{
    public class SkipInjectionAttributeTests
    {
        [Fact]
        public void DontInjectIntoSkippedMember()
        {
            var target = new TestMethodTarget();
            Assert.Equal(0, TestMethodTarget.Counter);

            var dummy = target.FirstProperty;
            Assert.Equal(0, TestMethodTarget.Counter);

            dummy = target.SecondProperty;
            Assert.Equal(1, TestMethodTarget.Counter);

            target.FirstEvent += (s, e) => { };
            Assert.Equal(1, TestMethodTarget.Counter);

            target.SecondEvent += (s, e) => { };
            Assert.Equal(2, TestMethodTarget.Counter);

            target.FirstMethod();
            Assert.Equal(2, TestMethodTarget.Counter);

            target.SecondMethod();
            Assert.Equal(3, TestMethodTarget.Counter);
        }

        [Fact]
        public void DontInjectIntoSkippedClass()
        {
            var target = new TestClassTarget();
            Assert.Equal(0, TestClassTarget.Counter);

            target.Method();
            Assert.Equal(0, TestClassTarget.Counter);
        }

        [TestMethodAspect]
        private class TestMethodTarget
        {
            public static int Counter = 0;

            [SkipInjection]
            public TestMethodTarget()
            {
            }

            [SkipInjection]
            public int FirstProperty { get; set; }

            public int SecondProperty { get; set; }

            [SkipInjection]
            public event EventHandler FirstEvent;

            public event EventHandler SecondEvent;

            [SkipInjection]
            public void FirstMethod()
            {
            }

            public void SecondMethod()
            {
            }
        }

        [TestClassAspect]
        [SkipInjection]
        private class TestClassTarget
        {
            public static int Counter = 0;

            public TestClassTarget()
            {
            }

            public void Method()
            {
            }
        }

        [Injection(typeof(TestMethodAspect))]
        [Aspect(Scope.Global)]
        public class TestMethodAspect : Attribute
        {
            [Advice(Kind.Before)]
            public void Before()
            {
                TestMethodTarget.Counter++;
            }
        }

        [Injection(typeof(TestClassAspect))]
        [Aspect(Scope.Global)]
        public class TestClassAspect : Attribute
        {
            [Advice(Kind.Before)]
            public void Before()
            {
                TestClassTarget.Counter++;
            }
        }
    }
}
