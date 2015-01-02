using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspectInjector.Broker;

namespace AspectInjector.Tests.AdviceInjectionTests
{
    [TestClass]
    public class NestedClassesTest
    {
        [TestMethod]
        public void Process_Nested_Classes()
        {
            Checker.Passed = false;
            var testClass = new TestNestedClass();
            testClass.Do();
            Assert.IsTrue(Checker.Passed);
        }

        [Aspect(typeof(TestAspect))]
        private class TestNestedClass
        {
            public void Do()
            {

            }
        }
    }

    public class TestAspect
    {
        [AdviceAttribute(InjectionPoints.Before, InjectionTargets.Method)]
        public void Do()
        {
            Checker.Passed = true;
        }
    }
}
