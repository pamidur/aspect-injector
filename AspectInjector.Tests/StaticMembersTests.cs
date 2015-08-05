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
            StaticMembersTests_Target.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(StaticMembersTests_BeforeMethodAspect))]
    internal static class StaticMembersTests_Target
    {
        public static void TestMethod()
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
