using AspectInjector.Broker;
using Xunit;
using System;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Advices
{

    public class AsyncTests
    {
        public static bool Data = false;

        [Fact]
        public void Advices_InjectAfterAsyncMethod()
        {
            AsyncTests.Data = false;
            Checker.Passed = false;

            var a = new AsyncTests_Target();
            a.Do().Wait();

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterAsyncMethod_WithResult()
        {
            AsyncTests.Data = false;
            Checker.Passed = false;

            var a = new AsyncTests_Target();
            var result = a.Do2().Result;

            Assert.Equal("test", result);
        }

        [Fact]
        public void Advices_InjectAfterAsyncMethod_Void()
        {
            AsyncTests.Data = false;
            Checker.Passed = false;

            var a = new AsyncTests_Target();
            a.Do3();
            Task.Delay(200).Wait();

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterAsyncMethod_WithArguments_And_Result()
        {
            AsyncTests.Data = false;
            Checker.Passed = false;

            var a = new AsyncTests_Target();
            a.Do4("args_test").ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.True(Checker.Passed);
        }
    }

    public class AsyncTests_Target
    {
        [AsyncTests_SimpleAspect]
        public async Task Do()
        {
            await Task.Delay(1);

            AsyncTests.Data = true;
        }

        [AsyncTests_SimpleAspectGlobal]
        public async Task<string> Do2()
        {
            await Task.Delay(1);

            AsyncTests.Data = true;

            return "test";
        }

        [AsyncTests_SimpleAspect]
        public async void Do3()
        {
            await Task.Delay(1);

            AsyncTests.Data = true;
        }

        [AsyncTests_ArgumentsAspect]
        public async Task<string> Do4(string testData)
        {
            await Task.Delay(1);

            AsyncTests.Data = true;

            return testData;
        }

        [Aspect(Aspect.Scope.Global)]
        [InjectionTrigger(typeof(AsyncTests_ArgumentsAspect))]
        public class AsyncTests_ArgumentsAspect : Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void AfterMethod([Advice.Argument(Advice.Argument.Source.ReturnValue)] object value,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args
                )
            {
                Checker.Passed = args[0].ToString() == "args_test" && value != null;
            }
        }

        [Aspect(Aspect.Scope.PerInstance)]
        [InjectionTrigger(typeof(AsyncTests_SimpleAspect))]
        public class AsyncTests_SimpleAspect : Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void AfterMethod(
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
                [Advice.Argument(Advice.Argument.Source.Instance)] object th
                )
            {
                Checker.Passed = AsyncTests.Data;
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [InjectionTrigger(typeof(AsyncTests_SimpleAspectGlobal))]
        public class AsyncTests_SimpleAspectGlobal : Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void AfterMethod(
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
                [Advice.Argument(Advice.Argument.Source.Instance)] object th

                )
            {
                Console.WriteLine(th.ToString());
                Checker.Passed = AsyncTests.Data;
            }
        }
    }
}