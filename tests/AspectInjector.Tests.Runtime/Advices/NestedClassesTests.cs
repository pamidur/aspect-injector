using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class NestedClassesTests
    {
        [TestMethod]
        public void Advices_InjectBeforeMethod_NestedClass()
        {
            Checker.Passed = false;
            var testClass = new NestedClassesTests_Target();
            testClass.Do();
            Assert.IsTrue(Checker.Passed);
        }

        [Cut(typeof(NestedClassesTests_Aspect))]
        private class NestedClassesTests_Target
        {
            public void Do()
            {
            }
        }
    }

    public class NestedClassesTests_Aspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void TestMethod()
        {
            Checker.Passed = true;
        }
    }
}