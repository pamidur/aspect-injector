using System;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Processors.ModuleProcessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests
{
    [TestClass]
    public class InjectionProcessorBaseClassTests : InjectionProcessorTestBase
    {
        [TestMethod]
        [Ignore] // please delete me if this isn't a desireable feature
        public void Finds_Ctor_AspectContexts_BaseClass()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == ".ctor" && c.TargetTypeContext.TypeDefinition.Name == "TestClass"));
        }

        [TestMethod]
        [Ignore] // please delete me if this isn't a desireable feature
        public void Finds_Method_AspectContexts_BaseClass()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();
            
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "VirtualMethodInBase"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do1"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do2"));
        }

        [TestMethod]
        [Ignore] // please delete me if this isn't a desireable feature
        public void Finds_Property_AspectContexts_BaseClass()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty"));
        }

        [TestMethod]
        [Ignore] // please delete me if this isn't a desireable feature
        public void Finds_Event_AspectContexts_BaseClass()
        {
            var contexts = InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent"));
        }

        [Aspect(typeof (TestAspectImplementation))]
        public abstract class TestBaseClass
        {
            public virtual void VirtualMethodInBase() { }
        }

        public class TestClass : TestBaseClass
        {
            public string TestProperty { get; set; }

            public event EventHandler TestEvent;

            public TestClass()
            {
            }

            public override void VirtualMethodInBase()
            {
                base.VirtualMethodInBase();
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