using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Interfaces
{

    public class InheritanceTests
    {
        [Fact]
        public void Interfaces_InjectionSupportsInheritance()
        {
            var ti = (IInheritanceTests)new InheritanceTests_Target();
            var r1 = ti.GetAspectType();

            var tib = (IInheritanceTests)new InheritanceTests_Base();
            var r2 = tib.GetAspectType();

            Assert.Equal(r1, r2);
        }

        [InheritanceTests_Aspect]
        public class InheritanceTests_Base { }

        [InheritanceTests_Aspect]
        public class InheritanceTests_Target : InheritanceTests_Base { }

        public interface IInheritanceTests
        {
            string GetAspectType();

            int GetAspectHash();
        }

        [Mixin(typeof(IInheritanceTests))]
        [Aspect(Scope.Global)]
        [Injection(typeof(InheritanceTests_Aspect))]
        public class InheritanceTests_Aspect : Attribute, IInheritanceTests
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