using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Advices
{

    public class FilterTests
    {
        [Fact]
        public void Advices_InjectAfterMethod_NameFilter()
        {
            Checker.Passed = false;

            var a = new FilterTests_Target();
            a.Do123();

            Assert.True(Checker.Passed);
        }

        [FilterTests_Aspect]
        public class FilterTests_Target
        {
            [FilterTests_Aspect]
            public void Do123()
            {
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(FilterTests_Aspect))]
        public class FilterTests_Aspect : Attribute
        {
            public int Counter = 0;

            [Advice(Kind.After, Targets = Target.Method)]
            public void AfterMethod()
            {
                Counter++;
                Checker.Passed = Counter == 1;
            }
        }
    }
}