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
        public void Doesnt_Find_Constructor_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(0, contexts.Count(c => c.TargetName == ".ctor"));
        }

        [TestMethod]
        public void Finds_Method_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "MethodInInterface"));
        }

        [TestMethod]
        public void Doesnt_Find_Method_AspectContexts_NoInterface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(0, contexts.Count(c => c.TargetName == "MethodNotInInterface"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "PropertyInInterface"));
        }

        [TestMethod]
        public void Doesnt_Find_Property_AspectContexts_NoInterface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(0, contexts.Count(c => c.TargetName == "PropertyNotInInterface"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_Interface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "EventInInterface"));
        }

        [TestMethod]
        public void Doesnt_Find_Event_AspectContexts_NoInterface()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(0, contexts.Count(c => c.TargetName == "EventNotInInterface"));
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

            public string PropertyNotInInterface { get; }

            public event EventHandler EventNotInInterface;

            public void MethodNotInInterface()
            {
            }
        }
    }
}