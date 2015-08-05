using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceArguments.Type
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void AdviceArguments_Type_Returns_Target_type()
        {
            Checker.Passed = false;
            TargetClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(AspectImplementation))]
    internal static class TargetClass
    {
        public static void TestMethod()
        {
        }
    }



    internal class AspectImplementation
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Type)] System.Type type)
        {
            Checker.Passed = type == typeof(TargetClass);
        }
    }
}
