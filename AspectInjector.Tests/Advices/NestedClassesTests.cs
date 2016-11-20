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

        [Aspect(typeof(NestedClassesTests_Aspect))]
        private class NestedClassesTests_Target
        {
            public void Do()
            {

            }
        }
    }

    public class NestedClassesTests_Aspect
    {
        [AdviceAttribute(InjectionPoints.Before, InjectionTargets.Method)]
        public void TestMethod()
        {
            Checker.Passed = true;
        }
    }
}
