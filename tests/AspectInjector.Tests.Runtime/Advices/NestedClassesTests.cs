using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Advices
{
    
    public class NestedClassesTests
    {
        [Fact]
        public void Advices_InjectBeforeMethod_NestedClass()
        {
            Checker.Passed = false;
            var testClass = new NestedClassesTests_Target();
            testClass.Do();
            Assert.True(Checker.Passed);
        }

        [NestedClassesTests_Aspect]
        private class NestedClassesTests_Target
        {
            public void Do()
            {
            }
        }
    }

    [Aspect(Aspect.Scope.Global)]
    [Injection(typeof(NestedClassesTests_Aspect))]
    public class NestedClassesTests_Aspect:Attribute
    {
        [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
        public void Fact()
        {
            Checker.Passed = true;
        }
    }
}