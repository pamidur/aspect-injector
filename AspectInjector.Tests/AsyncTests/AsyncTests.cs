using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    }

    public class TestClass
    {
        [Aspect(typeof(TestAspectImplementation))]
        public async Task Do()
        {
            await Task.Delay(200);

            TestAsyncMethods.Data = true;
        }

        //[Aspect(typeof(TestAspectImplementation))]
        //public async Task<string> Do2()
        //{
        //    await Task.Delay(200);

        //    TestAsyncMethods.Data = true;

        //    return "test";
        //}

        //[Aspect(typeof(TestAspectImplementation))]
        //public async void Do3()
        //{
        //    await Task.Delay(200);

        //    TestAsyncMethods.Data = true;
        //}
    }

    public class TestAspectImplementation
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod()
        {
            Checker.Passed = TestAsyncMethods.Data;
        }
    }
}