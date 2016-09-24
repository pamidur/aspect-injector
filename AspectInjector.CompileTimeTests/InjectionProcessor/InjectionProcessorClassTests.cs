using System;
using System.Linq;
using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    [TestClass]
    public class InjectionProcessorClassTests : AInjectionProcessorTest
    {
        [TestMethod]
        public void Finds_Ctor_AspectContexts_Class()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == ".ctor"));
        }

        [TestMethod]
        public void Finds_Method_AspectContexts_Class()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do1"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do2"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_Class()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_Class()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent"));
        }

        [Aspect(typeof(TestAspectImplementation))]
        public class TestClass
        {
            public string TestProperty { get; set; }

            public event EventHandler TestEvent;

            public TestClass()
            {
            }

            public object Do2(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }

            public static object Do1(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }
        }
    }
}