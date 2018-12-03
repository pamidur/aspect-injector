using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{
    public class AccessModifiersTests
    {
        private TestTarget _testClass = new TestTarget();

        [Fact]
        public void Advices_Inject_Into_Internal_Static()
        {
            Checker.Passed = false;
            TestTarget.InternalStatic();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_Inject_Into_Internal()
        {
            Checker.Passed = false;
            _testClass.Internal();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_Inject_Skips_Public()
        {
            Checker.Passed = false;
            _testClass.Public();
            Assert.False(Checker.Passed);
        }

        [AccessTestAspect]
        internal class TestTarget
        {
            public void Public()
            {
            }

            internal void Internal()
            {
            }

            internal static void InternalStatic()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(AccessTestAspect))]
        internal class AccessTestAspect : Attribute
        {
            [Advice(Advice.Kind.Before, WithAccess = AccessModifier.Internal | AccessModifier.Static | AccessModifier.Instance)]
            public void BeforeMethod()
            {
                Checker.Passed = true;
            }
        }
    }
}
