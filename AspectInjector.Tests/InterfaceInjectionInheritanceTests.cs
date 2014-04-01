using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InterfaceInjectionInheritanceTests
    {
        [TestMethod]
        public void InterfaceInjectionInheritanceTest()
        {
            var ti = (ITestInheritance)new TestInheritance();
            var r1 = ti.GetAspectType();

            var tib = (ITestInheritance)new TestInheritanceBase();
            var r2 = tib.GetAspectType();

            Assert.AreEqual(r1, r2);
        }

        [TestMethod]
        public void InterfaceInjectionAspectInheritanceTest()
        {
            var ti = (ITestInheritance)new TestInheritance();
            var r1 = ti.GetHashCode();

            var tib = (ITestInheritance)new TestInheritanceBase();
            var r2 = tib.GetHashCode();

            Assert.AreEqual(r1, r2);
        }
    }

    [Aspect(typeof(TestInheritanceAspect))]
    public class TestInheritance : TestInheritanceBase { }

    [Aspect(typeof(TestInheritanceAspect))]
    public class TestInheritanceBase { }

    public interface ITestInheritance
    {
        string GetAspectType();
        int GetAspectHash();
    }

    [AdviceInterfaceProxy(typeof(ITestInheritance))]
    public class TestInheritanceAspect : ITestInheritance
    {
        public string GetAspectType()
        {
            return GetType().ToString();
        }

        public int GetAspectHash()
        {
            return GetHashCode();
        }
    }
}
