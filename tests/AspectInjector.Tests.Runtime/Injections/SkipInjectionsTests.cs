using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Injections
{
    [TestSelfInjection]
    public class SkipInjectionsTests
    {
        [Fact]
        public void DontInjectIntoInjection()
        {
            var t = new TestTarget();
            t.Test();
            var i1 = new TestSelfInjection();
            i1.Test();
            var i2 = new TestOtherInjection();
            i2.Test();
            Assert.Equal(2, TestTarget.Counter);
        }

        private class TestTarget
        {
            public static int Counter = 0;

            public void Test() { }
        }

        [Injection(typeof(TestAspect))]
        private class TestSelfInjection : Attribute
        {
            public void Test() { }
        }

        [Injection(typeof(TestAspect))]
        private class TestOtherInjection : Attribute
        {
            public void Test() { }
        }

        [Aspect(Scope.Global)]
        public class TestAspect
        {
            [Advice(Kind.Before, Targets = Target.Method)]
            public void Before()
            {
                TestTarget.Counter++;
            }
        }
    }
}
