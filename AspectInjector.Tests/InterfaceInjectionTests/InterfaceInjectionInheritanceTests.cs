using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.InterfaceInjectionInheritance
{
    [TestClass]
    public class InterfaceInjectionInheritanceTests
    {
        [TestMethod]
        public void Interface_Injection_Supports_Inheritance()
        {
            var ti = (ITestInheritance)new TestInheritance();
            var r1 = ti.GetAspectType();

            var tib = (ITestInheritance)new TestInheritanceBase();
            var r2 = tib.GetAspectType();

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