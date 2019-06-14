using AspectInjector.Broker;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{


    public class CompilerGeneratedTargetsTests
    {
        [TestAspect]
        public class TestTarget
        {            
            public void Method()
            {
                int[] a = { 0, 1 };
                a.Single(x => x == 1);
            }

            public async Task AsyncMethod()
            {
                await Task.Delay(1);
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(TestAspect))]
        public class TestAspect : Attribute
        {
            public static int beforeCalls = 0;
            public static int afterCalls = 0;
            public static int aroundCalls = 0;

            [Advice(Kind.Before)]
            public void Before()
            {
                beforeCalls++;
            }

            [Advice(Kind.After)]
            public void After()
            {
                afterCalls++;
            }

            [Advice(Kind.Around)]
            public object Around([Argument(Source.Target)] Func<object[], object> target, [Argument(Source.Arguments)] object[] args)
            {
                aroundCalls++;
                return target(args);
            }

            public static void Reset()
            {
                beforeCalls = 0;
                afterCalls = 0;
                aroundCalls = 0;
            }
        }


        [Fact]
        public void Does_Not_Inject_Into_Anonymous_Methods()
        {
            var target = new TestTarget();
            TestAspect.Reset();
            target.Method();
            Assert.Equal(3, TestAspect.beforeCalls + TestAspect.afterCalls + TestAspect.aroundCalls);
        }

        [Fact]
        public void Does_Not_Inject_Into_Anonymous_AsyncStateMashines()
        {
            var target = new TestTarget();
            TestAspect.Reset();
            target.AsyncMethod().GetAwaiter().GetResult();
            Assert.Equal(3, TestAspect.beforeCalls + TestAspect.afterCalls + TestAspect.aroundCalls);
        }
    }
}
