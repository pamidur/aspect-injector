using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{
    public class OrderTests
    {
        private OrderTests_Target _beforeTestClass = new OrderTests_Target();

        [Fact]
        public void Advices_InjectBeforeMethod_Ordered()
        {
            Checker.Passed = false;
            _beforeTestClass.Fact();
            Assert.True(Checker.Passed);
        }


        internal class OrderTests_Target
        {
            [Trigger]
            public void Fact()
            {
            }
        }

        [Injection(typeof(OrderTests_Aspect1), Priority = 0)]
        [Injection(typeof(OrderTests_Aspect3), Priority = 2)]
        [Injection(typeof(OrderTests_Aspect2), Priority = 1)]
        class Trigger : Attribute { }

        [Aspect(Scope.Global)]
        public class OrderTests_Aspect1
        {
            [Advice(Kind.Before)]
            public void BeforeMethod()
            {
                Checker.Passed = false;
            }
        }

        [Aspect(Scope.Global)]
        public class OrderTests_Aspect2
        {
            [Advice(Kind.Before)]
            public void BeforeMethod()
            {
                Checker.Passed = false;
            }
        }

        [Aspect(Scope.Global)]
        public class OrderTests_Aspect3
        {
            [Advice(Kind.Before)]
            public void BeforeMethod()
            {
                Checker.Passed = true;
            }
        }
    }
}