using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Generics
{
    [TestClass]
    public class AdviceInjectionOpenGenericTests
    {
        private AdviceInjectionOpenGenericTests_Target<string, int> _target;

        [TestInitialize]
        public void SetUp()
        {
            _target = new AdviceInjectionOpenGenericTests_Target<string, int>();
        }

        [TestMethod]
        public void Inject_Advice_Before_OpenGenericClassMethod()
        {
            Checker.Passed = false;
            _target.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    internal class AdviceInjectionOpenGenericTests_TargetBase<T, U>
    {

    }

    [Aspect(typeof(AdviceInjectionOpenGenericTests_Aspect))]
    internal class AdviceInjectionOpenGenericTests_Target<U, V> :
        AdviceInjectionOpenGenericTests_TargetBase<string, U>
    {
        public void TestMethod()
        {
        }
    }

    internal class AdviceInjectionOpenGenericTests_Aspect
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }
}
