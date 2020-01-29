using AspectInjector.Broker;
using AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.InheritedInjections
{
    [Aspect(Scope.Global)]
    public class TestAspect
    {
        [Advice(Kind.Before)]
        public void Before()
        {
            Checker.Passed = true;
        }
    }

    [Injection(typeof(TestAspect), Inherited = true)]
    public abstract class BaseLocalAttribute : Attribute { }

    public class RealAttribute : BaseLocalAttribute { }

    public class RemoteAttribute : BaseAttribute { }

    internal class TestClass
    {
        [Real]
        [Remote]
        public void Do() { }
    }

    public class InheritedInjectionsTests
    {
        [Fact]
        public void InheritedInjection()
        {
            Checker.Passed = false;
            new TestClass().Do();

            Assert.True(Checker.Passed);
        }
    }
}
