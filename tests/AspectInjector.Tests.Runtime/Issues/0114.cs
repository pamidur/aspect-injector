using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AspectInjector.Tests.Runtime.Issues
{
    public class Issue_0114
    {
        [Fact]
        public void Test()
        {
            new TestClass().Do();
        }

        class TestClass
        {
            [TestInjection]
            public void Do() {
                Assert.True(false);
            }
        }

        [Aspect(Scope.Global)]
        public class TestAspect
        {
            [Advice(Kind.Around)]
            public object TestAdvice([Argument(Source.Triggers)] Attribute[] attributes)
            {
                Assert.NotEmpty(attributes);
                return null;
            }
        }

        [Injection(typeof(TestAspect))]
        public class TestInjection : Attribute
        {
            public TestInjection(string testArg = null)
            {

            }
        }
    }
}
