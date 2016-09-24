using System;
using System.Linq;
using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    [TestClass]
    public class InjectionProcessorBaseClassMemberTests : AInjectionProcessorTest
    {
        [TestMethod]
        public void Finds_Method_AspectContexts_BaseClassMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "VirtualMethodInBase" && c.TargetTypeContext.TypeDefinition.Name == "TestBaseClass"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "VirtualMethodInBase" && c.TargetTypeContext.TypeDefinition.Name == "TestClass"));
            
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "MethodInBase"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do1"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do2"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_BaseClassMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty" && c.TargetTypeContext.TypeDefinition.Name == "TestBaseClass"));
            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty" && c.TargetTypeContext.TypeDefinition.Name == "TestClass"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_BaseClassMember()
        {
            var contexts = BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(Module).ToArray();

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent" && c.TargetTypeContext.TypeDefinition.Name == "TestBaseClass"));
            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent" && c.TargetTypeContext.TypeDefinition.Name == "TestClass"));
        }
        
        public abstract class TestBaseClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            public virtual void VirtualMethodInBase() { }

            [Aspect(typeof(TestAspectImplementation))]
            public virtual void MethodInBase() { }

            [Aspect(typeof(TestAspectImplementation))]
            public virtual string TestProperty { get; set; }

            [Aspect(typeof(TestAspectImplementation))]
            public virtual event EventHandler TestEvent;
        }

        public class TestClass : TestBaseClass
        {
            public TestClass()
            {
            }

            public override string TestProperty { get; set; }

            public override event EventHandler TestEvent;

            public override void VirtualMethodInBase()
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