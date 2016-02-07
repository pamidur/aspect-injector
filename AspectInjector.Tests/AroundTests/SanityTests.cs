using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AroundTests
{
    [TestClass]
    public class SanityTests
    {
        [TestMethod]
        public void Aspect_Around_Wrapper_Does_Not_Show_In_StackTrace()
        {
            var passed = false;

            var a = new TestClass();

            try
            {
                a.Do();
            }
            catch (Exception e)
            {
                passed = !e.StackTrace.Contains("__a$");
            }

            Assert.IsTrue(passed);
        }

        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            public void Do()
            {
                throw new Exception("Test Exception");
            }
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return target(arguments);
            }
        }
    }
}