using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.InjectionAdviceBeforeOrder
{
    [TestClass]
    public class InjectionAdviceBeforeOrderTests
    {
        private TestClass _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new TestClass();
        }

        [TestMethod]
        public void Inject_Abortable_BeforeAspect_Latest()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }       
    }


    [Aspect(typeof(TestAspect1))]
    [Aspect(typeof(TestAspect2))]
    [Aspect(typeof(TestAspect3))]
    internal class TestClass
    {
        public void TestMethod()
        {
        }
    }

    internal class TestAspect1
    {       
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {

        }
    }

    internal class TestAspect2
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod([AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort)
        {
            abort = true;
        }
    }

    internal class TestAspect3
    {       
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = true;
        }
    }
}
