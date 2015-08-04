using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AsyncTests
{
    [TestClass]
    public class TestAsyncMethods
    {
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
        public async Task Do()
        {
            await Task.Run(() => { });

            Checker.Passed = false;
        }

        public Task DoWrapper()
        {
            var beforeTask = Task.Run(Before);
            var mainTask = beforeTask.ContinueWith(t => Do());
            mainTask.ContinueWith(Exception, TaskContinuationOptions.OnlyOnFaulted);
            mainTask.ContinueWith(After, TaskContinuationOptions.OnlyOnRanToCompletion);

            return beforeTask;
        }

        private void Exception(Task obj)
        {
            throw new NotImplementedException();
        }

        private Task Before()
        {
            throw new NotImplementedException();
        }

        private Task After(Task task)
        {
            throw new NotImplementedException();
        }
    }


    public class TestAspectImplementation
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod()
        {
            Checker.Passed = true;
        }
    }
}
