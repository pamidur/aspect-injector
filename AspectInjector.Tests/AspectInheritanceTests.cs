using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AspectInheritance
{
    [TestClass]
    public class AspectInheritanceTests
    {
        [TestMethod]
        [Ignore]
        public void AspectInheritanceTest()
        {
            var t = new TestAspectInheritance();

            var r1 = t.Hash();
            var r2 = t.BaseHash();

            Assert.AreNotEqual(r1, 0);
            Assert.AreNotEqual(r2, 0);

            Assert.AreEqual(r1, r2);
        }
    }

    [Aspect(typeof(TestAspectInheritanceAspect))]
    public class TestAspectInheritance : TestAspectInheritanceBase
    {
        public int Hash()
        {
            return 0;
        }
    }

    [Aspect(typeof(TestAspectInheritanceAspect))]
    public class TestAspectInheritanceBase
    {
        public int BaseHash()
        {
            return 0;
        }
    }

    public class TestAspectInheritanceAspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public int GetAspectHash([AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abortMethod)
        {
            abortMethod = true;
            return GetHashCode();
        }
    }
}