using AspectInjector.Broker;
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

        [Inject(typeof(NestedClassesTests_Aspect))]
        private class NestedClassesTests_Target
        {
            public void Do()
            {
            }
        }
    }

    [Aspect(Aspect.Scope.Global)]
    public class NestedClassesTests_Aspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Fact()
        {
            Checker.Passed = true;
        }
    }
}