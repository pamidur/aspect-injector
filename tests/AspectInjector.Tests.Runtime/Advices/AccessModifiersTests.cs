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

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(AccessTestAspect))]
        internal class AccessTestAspect : Attribute
        {
            [Advice(Advice.Kind.Around, WithAccess = AccessModifier.Internal | AccessModifier.AnyScope)]
            public object Around([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Assert.True(method.IsAssembly);
                Assert.False(method.IsPublic);
                return null;
            }
        }
    }
}
