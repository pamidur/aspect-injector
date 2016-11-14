using System;
using System.Linq;
using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    [TestClass]
    public class InjectionProcessorInterfaceMemberTests : AInjectionProcessorTest
    {
        [TestMethod]
        public void Finds_Method_AspectContexts_InterfaceMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "MethodInInterface"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_InterfaceMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "PropertyInInterface"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_InterfaceMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "EventInInterface"));
        }

        public interface ITestBaseInterface
        {
            [Aspect(typeof(TestAspectImplementation))]
            void MethodInInterface();
        }

        public interface ITestInterface : ITestBaseInterface
        {
            [Aspect(typeof(TestAspectImplementation))]
            string PropertyInInterface { get; }

            [Aspect(typeof(TestAspectImplementation))]
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