using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class OrderTests
    {
        private OrderTests_Target _beforeTestClass;

        [TestInitialize]
        public void SetUp()
        {
            _beforeTestClass = new OrderTests_Target();
        }

        [TestMethod, Ignore]//in release the order may be changed by compiller :(
        public void Advices_InjectBeforeMethod_Ordered()
        {
            Checker.Passed = false;
            _beforeTestClass.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Inject(typeof(OrderTests_Aspect1))]
    [Inject(typeof(OrderTests_Aspect2))]
    [Inject(typeof(OrderTests_Aspect3))]
    internal class OrderTests_Target
    {
        public void TestMethod()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class OrderTests_Aspect1
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class OrderTests_Aspect2
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = false;
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class OrderTests_Aspect3
    {
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void BeforeMethod()
        {
            Checker.Passed = true;
        }
    }
}