using System;
using System.Collections.Generic;
using System.Text;
using AspectInjector.Broker;
using Xunit;

namespace AspectInjector.Tests.Runtime.Injections
{
    public class InjectionOrderTests
    {
        [Fact]
        public void AspectInstance_InHierarchy_MustBeOne()
        {
            var target = new TestInjectionTarget();
            Assert.Equal(1, target.Test1());
            Assert.Equal(2, target.Test2());
        }

        [Aspect(Scope.PerInstance)]
        [Injection(typeof(TestAspect))]
        public class TestAspect : Attribute
        {
            public int Count { get; private set; }

            [Advice(Kind.Around, Targets = Target.Method)]
            public object Test(
                [Argument(Source.Arguments)] object[] arguments,
                [Argument(Source.Target)] Func<object[], object> target)
            {
                Count++;
                target(arguments);
                return Count;
            }
        }

        [TestAspect]
        private class TestInjectionTarget : TestInjectionTargetBase
        {
            public int Test2()
            {
                return 0;
            }
        }

        [TestAspect]
        private class TestInjectionTargetBase
        {
            public int Test1()
            {
                return 0;
            }
        }
    }
}
