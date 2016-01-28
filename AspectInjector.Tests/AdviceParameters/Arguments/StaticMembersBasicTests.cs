using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceParameters.Arguments
{
    [TestClass]
    public class StaticMembersBasicTests
    {
        [TestMethod]
        public void AdviceArguments_Arguments_InjectedCorrectly_on_StaticMethod()
        {
            Checker.Passed = false;
            TargetClass.TestMethod(1, "2");
            Assert.IsTrue(Checker.Passed);
        }

        internal static class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public static void TestMethod(int a, string b)
            {
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Arguments)] object[] args)
            {
                Checker.Passed = (int)args[0] == 1 && (string)args[1] == "2";
            }
        }
    }
}