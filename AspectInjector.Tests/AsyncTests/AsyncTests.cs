using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AsyncTests
{
    [TestClass]
    public class TestAsyncMethods
    {
        public static bool Data = false;

        [TestMethod]
        public void Aspect_Can_Be_Injected_Into_Async_Method()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Do().Wait();

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Aspect_Injected_Into_Async_Method_Can_Access_Args()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Do4("args_test").Wait();

            Assert.IsTrue(Checker.Passed);
        }
    }

    public class TestClass
    {
        private class Test1
        {
            public int[] a;
        }

        public void TestMethod(int data)
        {
            var a = new Test1();

            int[] args = new int[]
            {
                data
            };

            int[] b = args;

            a.a = b;
        }

        [Aspect(typeof(TestAspectImplementationSimple))]
        public async Task Do()
        {
            await Task.Delay(200);

            TestAsyncMethods.Data = true;
        }

        [Aspect(typeof(TestAspectImplementationSimple))]
        public async Task<string> Do2()
        {
            await Task.Delay(200);

            TestAsyncMethods.Data = true;

            return "test";
        }

        [Aspect(typeof(TestAspectImplementationSimple))]
        public async void Do3()
        {
            await Task.Delay(200);

            TestAsyncMethods.Data = true;
        }

        [Aspect(typeof(TestAspectImplementation))]
        public async Task<string> Do4(string testData)
        {
            await Task.Delay(200);

            TestAsyncMethods.Data = true;

            return testData;
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.After, InjectionTargets.Method)]
            public void AfterMethod([AdviceArgument(AdviceArgumentSource.ReturnValue)] object value,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] args
                )
            {
                Checker.Passed = args[0].ToString() == "args_test";
            }
        }

        public class TestAspectImplementationSimple
        {
            [Advice(InjectionPoints.After, InjectionTargets.Method)]
            public void AfterMethod([AdviceArgument(AdviceArgumentSource.ReturnValue)] object value,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] args
                )
            {
                Checker.Passed = TestAsyncMethods.Data;
            }
        }
    }
}