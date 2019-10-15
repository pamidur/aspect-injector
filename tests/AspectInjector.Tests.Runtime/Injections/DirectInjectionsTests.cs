using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Injections
{
    public class DirectInjectionsTests
    {
        [Fact]
        public void CanInjectIntoGetterDirectly()
        {
            Checker.Passed = false;
            var t = new TestTarget();
            var a = t.Text;
            Assert.True(Checker.Passed);
            Checker.Passed = false;
        }

        private class TestTarget
        {
            public string Text { [TestAspect] get; set; }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(TestAspect))]
        private class TestAspect : Attribute
        {
            private int _count = 0;

            [Advice(Kind.Before)]
            public void Before()
            {
                _count++;
                Assert.Equal(1, _count);
                Checker.Passed = true;
            }
        }
    }
}
