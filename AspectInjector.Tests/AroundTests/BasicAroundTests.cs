using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AroundTests
{
    [TestClass]
    internal class BasicAroundTests
    {
        [TestMethod]
        public void Aspect_Can_Wrap_Method_Around()
        {
            Checker.Passed = false;

            var a = new TestClass();

            Int32 v = 2;
            object vv = v;

            a.Do123((System.Int32)vv, new StringBuilder(), new object());

            Assert.IsTrue(Checker.Passed);
        }

        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            public int Do123(int data, StringBuilder sb, object to)
            {
                var a = 1;
                var b = a + data;
                return b;
            }
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public void AfterMethod()
            {
                Checker.Passed = true;
            }
        }
    }
}