using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
    [TestClass]
    public class AdviceOrderedInjectionBeforeTests
    {
        private AdviceOrderedInjectionBeforeTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new AdviceOrderedInjectionBeforeTests_Target();
        }

        [TestMethod, Ignore]//in release the order may be changed by compiller :(
        public void Injected_Advices_Executed_In_Order_Stated()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(AdviceOrderedInjectionBeforeTests_Aspect1))]
    [Aspect(typeof(AdviceOrderedInjectionBeforeTests_Aspect2))]
    [Aspect(typeof(AdviceOrderedInjectionBeforeTests_Aspect3))]
    internal class AdviceOrderedInjectionBeforeTests_Target
    {
        public void TestMethod()
        {
        }
    }

    internal class AdviceOrderedInjectionBeforeTests_Aspect1
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
        }
    }

    internal class AdviceOrderedInjectionBeforeTests_Aspect2
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = false;
        }
    }

    internal class AdviceOrderedInjectionBeforeTests_Aspect3
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = true;
        }
    }
}