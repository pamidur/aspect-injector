using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Generics
{
    [TestClass]
    public class AdviceInjectionGenericTests
    {
        private AdviceInjectionGenericTests_Target<string> _target;

        [TestInitialize]
        public void SetUp()
        {
            _target = new AdviceInjectionGenericTests_Target<string>();
        }

        [TestMethod]
        public void Inject_Advice_Before_GenericClassMethod()
        {
            Checker.Passed = false;
            _target.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Inject_Advice_Before_GenericMethod()
        {
            Checker.Passed = false;
            _target.TestGenericMethod<object>();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(AdviceInjectionGenericTests_Aspect))]
    internal class AdviceInjectionGenericTests_Target<T>
    {
        public void TestMethod()
        {
        }

        public void TestGenericMethod<U>()
        {
        }
    }

    internal class AdviceInjectionGenericTests_Aspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }
}
