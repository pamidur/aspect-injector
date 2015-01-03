using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests
{
    [TestClass]
    public class InterfaceInjectionInheritanceTests
    {
        [TestMethod]
        public void Interface_Injection_Supports_Inheritance()
        {
            var ti = (IInterfaceInjectionInheritanceTests)new InterfaceInjectionInheritanceTests_Target();
            var r1 = ti.GetAspectType();

            var tib = (IInterfaceInjectionInheritanceTests)new InterfaceInjectionInheritanceTests_Base();
            var r2 = tib.GetAspectType();

            Assert.AreEqual(r1, r2);
        }
    }

    [Aspect(typeof(InterfaceInjectionInheritanceTests_Aspect))]
    public class InterfaceInjectionInheritanceTests_Base { }

    [Aspect(typeof(InterfaceInjectionInheritanceTests_Aspect))]
    public class InterfaceInjectionInheritanceTests_Target : InterfaceInjectionInheritanceTests_Base { }

    public interface IInterfaceInjectionInheritanceTests
    {
        string GetAspectType();

        int GetAspectHash();
    }

    [AdviceInterfaceProxy(typeof(IInterfaceInjectionInheritanceTests))]
    public class InterfaceInjectionInheritanceTests_Aspect : IInterfaceInjectionInheritanceTests
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