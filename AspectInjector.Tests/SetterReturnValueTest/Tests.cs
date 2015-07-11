using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.SetterReturnValueTest
{
    [TestClass]
    public class Tests
    {
        public static int Data = 0;

        [TestMethod]
        public void Setter_After_Has_Access_To_OldValue()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.DataAfter = 2;
            Data = 2;
            a.DataAfter = 4;

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Setter_Before_Has_Access_To_OldValue()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.DataBefore = 2;
            Data = 2;
            a.DataBefore = 4;

            Assert.IsTrue(Checker.Passed);
        }
    }


    public class TestClass
    {
        [Aspect(typeof(TestAspectAfter))]
        public int DataAfter { get; set; }

        [Aspect(typeof(TestAspectBefore))]
        public int DataBefore { get; set; }
    }

    public class TestAspectAfter
    {
        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.TargetValue)] object old)
        {
            Checker.Passed = (int)old == Tests.Data;
        }
    }

    public class TestAspectBefore
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void AfterMethod(
            [AdviceArgument(AdviceArgumentSource.TargetValue)] object old,
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abortFlag
            )
        {
            Checker.Passed = (int)old == Tests.Data;
        }
    }
}
