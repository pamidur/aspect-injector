using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InjectionAdviceBeforeOrderTests
    {
        private InjectionAdviceBeforeOrderTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new InjectionAdviceBeforeOrderTests_Target();
        }

        [TestMethod]
        public void Inject_Abortable_BeforeAspect_Latest()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }       
    }


    [Aspect(typeof(InjectionAdviceBeforeOrderTests_Aspect1))]
    [Aspect(typeof(InjectionAdviceBeforeOrderTests_Aspect2))]
    [Aspect(typeof(InjectionAdviceBeforeOrderTests_Aspect3))]
    internal class InjectionAdviceBeforeOrderTests_Target
    {
        public void TestMethod()
        {
        }
    }

    internal class InjectionAdviceBeforeOrderTests_Aspect1
    {       
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {

        }
    }

    internal class InjectionAdviceBeforeOrderTests_Aspect2
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod([AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort)
        {
            abort = true;
        }
    }

    internal class InjectionAdviceBeforeOrderTests_Aspect3
    {       
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = true;
        }
    }
}
