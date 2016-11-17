using AspectInjector.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Tests
{
    [TestClass]
    public class TypeConversionTests
    {
        [TestMethod]
        public void Open_Generic_FQN_Matches()
        {
            var type = typeof(TestType<>.TestClass<,>);

            var module = ModuleDefinition.CreateModule("main", ModuleKind.Dll);
            var tr = module.Import(type);

            var fqn1 = FQN.FromType(type);
            var fqn2 = FQN.FromTypeReference(tr);

            Assert.AreEqual(fqn1.ToString(), fqn2.ToString());
        }

        [TestMethod]
        public void FQN_Matches()
        {
            var type = typeof(TypeConversionTests);

            var module = ModuleDefinition.CreateModule("main", ModuleKind.Dll);
            var tr = module.Import(type);

            var fqn1 = FQN.FromType(type);
            var fqn2 = FQN.FromTypeReference(tr);

            Assert.AreEqual(fqn1.ToString(), fqn2.ToString());
        }

        [TestMethod]
        public void Closed_Generic_FQN_Matches()
        {
            var type = typeof(TestType<List<int>>.TestClass<string, short>);

            var module = ModuleDefinition.CreateModule("main", ModuleKind.Dll);
            var tr = module.Import(type);

            var fqn1 = FQN.FromType(type);
            var fqn2 = FQN.FromTypeReference(tr);

            Assert.AreEqual(fqn1.ToString(), fqn2.ToString());
        }

        [TestMethod]
        public void Can_Parse_FQN_With_Generics_String()
        {
            var type = typeof(TestType<List<int>>.TestClass<string, short>);
            var fqn1 = FQN.FromType(type);
            var fqnStr = fqn1.ToString();
            var fqn2 = FQN.FromString(fqnStr);

            Assert.AreEqual(fqnStr, fqn2.ToString());
        }

        [TestMethod]
        public void Can_Parse_FQN_With_Open_Generics_String()
        {
            var type = typeof(TestType<>.TestClass<,>);
            var fqn1 = FQN.FromType(type);
            var fqnStr = fqn1.ToString();
            var fqn2 = FQN.FromString(fqnStr);

            Assert.AreEqual(fqnStr, fqn2.ToString());
        }

        [TestMethod]
        public void Can_Parse_FQN_String()
        {
            var type = typeof(TypeConversionTests);
            var fqn1 = FQN.FromType(type);
            var fqnStr = fqn1.ToString();
            var fqn2 = FQN.FromString(fqnStr);

            Assert.AreEqual(fqnStr, fqn2.ToString());
        }

        public class TestType<T1>
        {
            public class TestClass<T2, T3>
            {
            }
        }
    }
}