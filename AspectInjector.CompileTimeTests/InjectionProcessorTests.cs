using System;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Processors.ModuleProcessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests
{
    [TestClass]
    public class InjectionProcessorTests : InjectionProcessorTestBase
    {
        [TestMethod]
        public void Finds_Ctor_AspectContexts_Member()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == ".ctor"));
        }

        [TestMethod]
        public void Finds_Method_AspectContexts_Member()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do1"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do2"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_Member()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_Member()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent"));
        }

        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            public string TestProperty { get; set; }

            [Aspect(typeof (TestAspectImplementation))]
            public event EventHandler TestEvent;

            [Aspect(typeof(TestAspectImplementation))]
            public TestClass()
            {
            }

            [Aspect(typeof(TestAspectImplementation))]
            public object Do2(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }

            [Aspect(typeof(TestAspectImplementation))]
            public static object Do1(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }
        }
    }
}