using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class GenericTests
    {
        private GenericTests_Target<string> _target;
        private GenericTests_OpenGenericTarget<string, int> _openGenericTarget;

        [TestInitialize]
        public void SetUp()
        {
            _target = new GenericTests_Target<string>();
            _openGenericTarget = new GenericTests_OpenGenericTarget<string, int>();
        }

        [TestMethod]
        public void Advices_InjectBeforeMethod_GenericClass()
        {
            Checker.Passed = false;
            _target.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeMethod_GenericMethod()
        {
            Checker.Passed = false;
            _target.TestGenericMethod<object>();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectBeforeMethod_OpenGenericMethod()
        {
            Checker.Passed = false;
            _openGenericTarget.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAroundMethod_GenericMethod()
        {
            Checker.Passed = false;
            var target = new GenericAroundTests_Target();
            var rr = string.Empty;
            target.TestMethod<string>(ref rr);
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Inject(typeof(GenericTests_Aspect))]
    internal class GenericTests_Target<T>
    {
        public void TestMethod()
        {
        }

        public void TestGenericMethod<U>()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericTests_Aspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    internal class GenericTests_OpenGenericTargetBase<T, U>
    {
    }

    [Inject(typeof(GenericTests_OpenGenericAspect))]
    internal class GenericTests_OpenGenericTarget<U, V> :
        GenericTests_OpenGenericTargetBase<string, U>
    {
        public void TestMethod()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericTests_OpenGenericAspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    [Inject(typeof(GenericAroundTests_Aspect))]
    internal class GenericAroundTests_Target
    {
        public T TestMethod<T>(ref T value)
        {
            var a = new object[] { value };
            return value;
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class GenericAroundTests_Aspect
    {
        [Advice(Advice.Type.Around, Advice.Target.Method)]
        public object AroundAdvice([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
        {
            Checker.Passed = true;
            return target(arguments);
        }
    }
}