using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }

    [Cut(typeof(GenericTests_Aspect))]
    internal class GenericTests_Target<T>
    {
        public void TestMethod()
        {
        }

        public void TestGenericMethod<U>()
        {
        }
    }

    internal class GenericTests_Aspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }

    internal class GenericTests_OpenGenericTargetBase<T, U>
    {
    }

    [Cut(typeof(GenericTests_OpenGenericAspect))]
    internal class GenericTests_OpenGenericTarget<U, V> :
        GenericTests_OpenGenericTargetBase<string, U>
    {
        public void TestMethod()
        {
        }
    }

    internal class GenericTests_OpenGenericAspect
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod() { Checker.Passed = true; }
    }
}