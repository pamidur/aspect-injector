using System;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace AspectInjector.CompileTimeTests.InjectionProcessor
{
    [TestClass]
    public class InjectionProcessorAssemblyTests : AInjectionProcessorTest
    {
        private ModuleDefinition GetModuleWithAssemblyAttribute()
        {
            var attributeConstructor = Module.Import(typeof (AspectAttribute).GetConstructor(new[] {typeof (Type)}));
            var customAttribute = new CustomAttribute(attributeConstructor);
            var argument = new CustomAttributeArgument(Module.Import(typeof (TypeReference)), Module.Import(typeof (TestAspectImplementation)));
            customAttribute.ConstructorArguments.Add(argument);

            Module.CustomAttributes.Add(customAttribute);
            return Module;
        }

        private AspectContext[] GetAspectContexts(string testClassName)
        {
            return BuildTask.Processors.ModuleProcessors.InjectionProcessor.GetAspectContexts(GetModuleWithAssemblyAttribute())
                                     .Where(c => c.TargetTypeContext.TypeDefinition.Name == testClassName)
                                     .ToArray();
        }

        [TestMethod]
        public void Finds_Ctor_AspectContexts_Assembly()
        {
            var contexts = GetAspectContexts(nameof(TestClass));

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == ".ctor"));
        }

        [TestMethod]
        public void Finds_Method_AspectContexts_Assembly()
        {
            var contexts = GetAspectContexts(nameof(TestClass));

            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do1"));
            Assert.AreEqual(1, contexts.Count(c => c.TargetName == "Do2"));
        }

        [TestMethod]
        public void Finds_Property_AspectContexts_Assembly()
        {
            var contexts = GetAspectContexts(nameof(TestClass));

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestProperty"));
        }

        [TestMethod]
        public void Finds_Event_AspectContexts_Assembly()
        {
            var contexts = GetAspectContexts(nameof(TestClass));

            Assert.AreEqual(2, contexts.Count(c => c.TargetName == "TestEvent"));
        }

        public class TestClass
        {
            public string TestProperty { get; set; }

            public event EventHandler TestEvent;

            public object Do2(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef, ref double doubleRef,
                ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }

            public static object Do1(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut, ref long longRef,
                ref double doubleRef, ref char charRef)
            {
                objOut = new object();
                valueOut = 1;

                return new object();
            }
        }
    }
}