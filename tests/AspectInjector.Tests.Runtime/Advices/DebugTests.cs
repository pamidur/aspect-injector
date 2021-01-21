using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{
    public class DebugTests
    {
        [Fact(/*Skip = "Only debug"*/)]
        public void DebugWorks()
        {
            new TestClass().Do();
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(TestAspect))]
        public class TestAspect : Attribute
        {
            [Advice(Kind.Around)]
            public object Around([Argument(Source.Arguments)] object[] args, [Argument(Source.Target)] Func<object[],object> target)
            {
                return target(args);
            }
        }

        public class TestClass
        {
            [TestAspect]
            public void Do()
            {
                System.Diagnostics.Trace.WriteLine("Test");
            }
        }
    }
}
