using AspectInjector.Broker;
using System;
using System.Reflection;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{
    public class AccessModifiersTests
    {
        private TestTarget _testClass = new TestTarget();

        [Fact]
        public void Advices_Inject_Into_Internal_Static()
        {
            TestTarget.InternalStatic();
        }

        [Fact]
        public void Advices_Inject_Into_Internal()
        {
            _testClass.Internal();
        }

        [Fact]
        public void Advices_Inject_Skips_Public()
        {
            _testClass.Public();
        }

        [AccessTestAspect]
        internal class TestTarget
        {
            public void Public() { }
            internal void Internal() => Assert.True(false);
            internal static void InternalStatic() => Assert.True(false);
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(AccessTestAspect))]
        internal class AccessTestAspect : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Internal)]
            public object TestAccess([Argument(Source.Method)] MethodBase method)
            {
                Assert.True(method.IsAssembly);
                Assert.False(method.IsPublic);
                return null;
            }
        }
    }
}
