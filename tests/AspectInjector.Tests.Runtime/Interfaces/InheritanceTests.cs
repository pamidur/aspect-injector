using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Interfaces
{
    [TestClass]
    public class InheritanceTests
    {
        [TestMethod]
        public void Interfaces_InjectionSupportsInheritance()
        {
            var ti = (IInheritanceTests)new InheritanceTests_Target();
            var r1 = ti.GetAspectType();

            var tib = (IInheritanceTests)new InheritanceTests_Base();
            var r2 = tib.GetAspectType();

            Assert.AreEqual(r1, r2);
        }

        [Inject(typeof(InheritanceTests_Aspect))]
        public class InheritanceTests_Base { }

        [Inject(typeof(InheritanceTests_Aspect))]
        public class InheritanceTests_Target : InheritanceTests_Base { }

        public interface IInheritanceTests
        {
            string GetAspectType();

            int GetAspectHash();
        }

        [Mixin(typeof(IInheritanceTests))]
        [Aspect(Aspect.Scope.Global)]
        public class InheritanceTests_Aspect : IInheritanceTests
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
}