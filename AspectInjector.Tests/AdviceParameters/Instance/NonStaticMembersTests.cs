using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceParameters.Instance
{
    [TestClass]
    public class NonStaticMembersTests
    {
        [TestMethod]
        public void AdviceArguments_Instance_Not_Null_on_NonStaticMethod()
        {
            Checker.Passed = false;
            new TargetClass().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod()
            {
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
            {
                Checker.Passed = instance != null;
            }
        }
    }
}