using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
    [TestClass]
    public class SetterValueTests
    {
        public static int Data = 0;

        [TestMethod]
        public void Setter_After_Has_Access_To_OldValue()
        {
            Checker.Passed = false;

            var a = new SetterValueTests_Target();
            a.DataAfter = 2;
            Data = 2;
            a.DataAfter = 4;

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Setter_Before_Has_Access_To_OldValue()
        {
            Checker.Passed = false;

            var a = new SetterValueTests_Target();
            a.DataBefore = 2;
            Data = 2;
            a.DataBefore = 4;

            Assert.IsTrue(Checker.Passed);
        }
    }


    public class SetterValueTests_Target
    {
        [Aspect(typeof(SetterValueTests_AfterAspect))]
        public int DataAfter { get; set; }

        [Aspect(typeof(SetterValueTests_BeforeAspect))]
        public int DataBefore { get; set; }
    }

    public class SetterValueTests_AfterAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.TargetValue)] object old)
        {
            Checker.Passed = (int)old == SetterValueTests.Data;
        }
    }

    public class SetterValueTests_BeforeAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Setter)]
        public void AfterMethod(
            [AdviceArgument(AdviceArgumentSource.TargetValue)] object old,
            [AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abortFlag
            )
        {
            Checker.Passed = (int)old == SetterValueTests.Data;
        }
    }
}
