using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceParameters.Instance
{
    [TestClass]
    public class StaticMembersTests
    {
        [TestMethod]
        public void AdviceArguments_Instance_Null_on_StaticMethod()
        {
            Checker.Passed = false;
            TargetClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        internal static class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public static void TestMethod()
            {
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
            {
                Checker.Passed = instance == null;
            }
        }
    }
}