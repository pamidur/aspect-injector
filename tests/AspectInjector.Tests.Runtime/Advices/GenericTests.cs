using AspectInjector.Broker;
using Xunit;
using System;

namespace AspectInjector.Tests.Advices
{
    
    public class GenericTests
    {
        private GenericTests_Target<string> _target = new GenericTests_Target<string>();
        private GenericTests_OpenGenericTarget<string, int> _openGenericTarget = new GenericTests_OpenGenericTarget<string, int>();
        
        [Fact]
        public void Advices_InjectBeforeMethod_GenericClass()
        {
            Checker.Passed = false;
            _target.Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeMethod_GenericMethod()
        {
            Checker.Passed = false;
            _target.TestGenericMethod<object>();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectBeforeMethod_OpenGenericMethod()
        {
            Checker.Passed = false;
            _openGenericTarget.Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAroundMethod_GenericMethod()
        {
            Checker.Passed = false;
            var target = new GenericAroundTests_Target();
            var rr = string.Empty;
            target.Fact<string>(ref rr);
            Assert.True(Checker.Passed);
        }
    }

    [Inject(typeof(GenericTests_Aspect))]
    internal class GenericTests_Target<T>
    {
        public void Fact()
        {
        }

        public void TestGenericMethod<U>()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericTests_Aspect
    {
        [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    internal class GenericTests_OpenGenericTargetBase<T, U>
    {
    }

    [Inject(typeof(GenericTests_OpenGenericAspect))]
    internal class GenericTests_OpenGenericTarget<U, V> :
        GenericTests_OpenGenericTargetBase<string, U>
    {
        public void Fact()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericTests_OpenGenericAspect
    {
        [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    [Inject(typeof(GenericAroundTests_Aspect))]
    internal class GenericAroundTests_Target
    {
        public T Fact<T>(ref T value)
        {
            var a = new object[] { value };
            return value;
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericAroundTests_Aspect
    {
        [Advice(Advice.Kind.Around, Targets = Advice.Target.Method)]
        public object AroundAdvice([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
        {
            Checker.Passed = true;
            return target(arguments);
        }
    }
}