using AspectInjector.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Tests
{
    [TestClass]
    public class GenericProcessingTests
    {
        private TypeDefinition testClass;

        [TestInitialize]
        public void Init()
        {
            testClass = AssemblyDefinition.ReadAssembly(typeof(GenericProcessingTests).Assembly.Location).MainModule.GetTypes().First(t => t.Name == typeof(TestClass<>).Name && t.DeclaringType?.Name == typeof(GenericProcessingTests).Name);
        }

        [TestMethod]
        public void Can_Resolve_Open_Generic_Method()
        {
            var method = testClass.Methods.First(m => m.Name.StartsWith("Method1"));
            var genericMethod = method.MakeHostInstanceGeneric(testClass);
            Assert.IsNotNull(genericMethod.Resolve());
        }

        [TestMethod]
        public void Can_Resolve_Closed_Generic_Method()
        {
            var method = testClass.Methods.First(m => m.Name.StartsWith("Method2"));
            var genericClass = testClass.MakeGenericInstanceType(testClass.Module.Import(typeof(StringComparer)));
            var genericMethod = method.MakeHostInstanceGeneric(genericClass);
            Assert.IsNotNull(genericMethod.Resolve());
        }

        [TestMethod]
        public void Can_Resolve_Mixed_Generic_Method()
        {
            var method = testClass.Methods.First(m => m.Name.StartsWith("Method3"));
            var genericClass = testClass.MakeGenericInstanceType(testClass.Module.Import(typeof(StringComparer)));
            var genericMethod = method.MakeHostInstanceGeneric(genericClass);
            Assert.IsNotNull(genericMethod.Resolve());
        }

        public class TestClass<R>
        {
            public T Method1<T>(T p1)
            {
                return p1;
            }

            public R Method2(R p1)
            {
                return default(R);
            }

            public Tuple<R,T> Method3<T>(R p1, T p2, Tuple<R,T> p3)
            {
                return null;
            }
        }
    }
}
