using AspectInjector.Broker;
using AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.InheritedInjections
{
    [Aspect(Scope.Global)]
    public class TestAspect
    {
        [Advice(Kind.Around)]
        public object Around(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Triggers)] Attribute[] attrs)
        {
            var res = target(args);
            Checker.Passed = true;
            return res;
        }
    }

    [Injection(typeof(TestAspect), Inherited = true)]
    public abstract class BaseLocalAttribute : Attribute {
        public int Value { get; set; }
        public int Value2;
    }

    public class RealAttribute : BaseLocalAttribute { }

    public class RemoteAttribute : BaseAttribute { }

    internal class TestClass
    {
        [Real(Value = 1, Value2 = 2)]
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
