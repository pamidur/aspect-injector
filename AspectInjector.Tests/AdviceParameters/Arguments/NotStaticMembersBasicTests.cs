using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceParameters.Arguments
{
    [TestClass]
    public class NotStaticMembersBasicTests
    {
        [TestMethod]
        public void AdviceArguments_Arguments_InjectedCorrectly_on_StaticMethod()
        {
            Checker.Passed = false;
            new TargetClass().TestMethod(1, "2");
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod(int a, string b)
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