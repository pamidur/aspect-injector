//using AspectInjector.Broker;
//using Xunit;

//namespace AspectInjector.Tests.Advices
//{
//    public class OrderTests
//    {
//        private OrderTests_Target _beforeTestClass = new OrderTests_Target();

//        [Fact(Skip = "In release the order may be changed by compiller :(")]
//        public void Advices_InjectBeforeMethod_Ordered()
//        {
//            Checker.Passed = false;
//            _beforeTestClass.Fact();
//            Assert.True(Checker.Passed);
//        }
//    }

//    [Inject(typeof(OrderTests_Aspect1))]
//    [Inject(typeof(OrderTests_Aspect2))]
//    [Inject(typeof(OrderTests_Aspect3))]
//    internal class OrderTests_Target
//    {
//        public void Fact()
//        {
//        }
//    }

//    [Aspect(Aspect.Scope.Global)]
//    internal class OrderTests_Aspect1
//    {
//        [Advice(Advice.Type.Before, Advice.Target.Method)]
//        public void BeforeMethod()
//        {
//        }
//    }

//    [Aspect(Aspect.Scope.Global)]
//    internal class OrderTests_Aspect2
//    {
//        [Advice(Advice.Type.Before, Advice.Target.Method)]
//        public void BeforeMethod()
//        {
//            Checker.Passed = false;
//        }
//    }

//    [Aspect(Aspect.Scope.Global)]
//    internal class OrderTests_Aspect3
//    {
//        [Advice(Advice.Type.Before, Advice.Target.Method)]
//        public void BeforeMethod()
//        {
//            Checker.Passed = true;
//        }
//    }
//}