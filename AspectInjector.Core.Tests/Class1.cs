using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Tests
{
    [TestClass]
    public class TypeConversionTests
    {
        [TestMethod]
        public void Type_Converts_To_FQN_Conversion()
        {
            TestMethod<string>();
        }

        private void TestMethod<H>()
        {
            var type = typeof(TestType<>.TestClass<>);

            var module = ModuleDefinition.CreateModule("main", ModuleKind.Dll);
            var tr = module.Import(type);

            var fqn1 = FQN.FromType(type);
            var fqn2 = FQN.FromTypeReference(tr);

            Assert.AreEqual(fqn1.ToString(), fqn2.ToString());
        }

        public class Test1<T>
        {
        }

        public class TestType<T> : Test1<int>
            where T : IEnumerable
        {
            public class TestClass<TR> : Test1<TR>
            {
            }
        }
    }
}