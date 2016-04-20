using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
    [TestClass]
    public class StaticMembersTests
    {
        [TestMethod]
        public void Inject_Before_StaticMethod()
        {
            Checker.Passed = false;
            StaticMembersTests_Target.TestStaticMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(StaticMembersTests_BeforeMethodAspect))]
    internal class StaticMembersTests_Target
    {
        public static void TestStaticMethod()
        {
        }

        public void TestInstanceMethod()
        {
        }
    }

    internal class StaticMembersTests_BeforeMethodAspect
    {
        //Property
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }
}
