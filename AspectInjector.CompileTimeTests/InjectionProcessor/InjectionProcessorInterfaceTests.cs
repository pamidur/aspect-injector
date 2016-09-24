using System;
using System.Linq;
using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    [TestClass]
    public class InjectionProcessorInterfaceTests : AInjectionProcessorTest
    {
        [TestMethod]
        public void Finds_Constructor_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            // the ctor should not inherit the attribute from an interface
            Assert.AreEqual(0, contexts.Count(c => c.TargetName == ".ctor"));
        }

        [TestMethod]
        public void Finds_Method_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "MethodInInterface"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "PropertyInInterface"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "EventInInterface"));
        }

        [Aspect(typeof(TestAspectImplementation))]
        public interface ITestBaseInterface
        {
            void MethodInInterface();
        }

        public interface ITestInterface : ITestBaseInterface
        {
            string PropertyInInterface { get; }

            event EventHandler EventInInterface;
        }

        public class TestClass : ITestInterface
        {
            public void MethodInInterface()
            {
            }

            public string PropertyInInterface { get; }

            public event EventHandler EventInInterface;
        }
    }
}