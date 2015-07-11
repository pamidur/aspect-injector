using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.SetterReturnValueTest
{
    [TestClass]
    public class Tests
    {
        public static int Data = 0;

        [TestMethod]
        public void Setter_Has_Access_To_OldValue()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Data = 2;
            Data = 2;
            a.Data = 4;

            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(TestAspect))]
    public class TestClass
    {
        public int Data { get; set; }
    }

    public class TestAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Setter)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.TargetValue)] object old)
        {
            Checker.Passed = (int)old == Tests.Data;
        }
    }
}
