using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AspectInjector.Tests.Runtime.Injections
{
    public class InterfaceTriggers
    {
        [Fact]
        public void Interface_Triggers_Work()
        {
            Checker.Passed = false;
            new TestTarget().Do();
            Assert.True(Checker.Passed);
        }

        [Aspect(Scope.Global)]
        public class TestAspect
        {
            [Advice(Kind.Before, Targets = Target.Method)]
            public void Before()
            {
                Checker.Passed = true;
            }
        }

        [Injection(typeof(TestAspect))]
        public interface INeedInjection
        {

        }

        public class TestTarget : INeedInjection
        {
            public void Do()
            {

            }
        }
    }
}
