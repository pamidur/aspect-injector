//using AspectInjector.Broker;
//using AspectInjector.Tests.Assets;
//using System;
//using Xunit;

//namespace AspectInjector.Tests.Runtime.Issues
//{
//    public class Issue_0097
//    {
//        [Fact]
//        public void Fixed()
//        {
//            new Target().Value = 12;
//        }

//        private class TargetBase<T>
//        {
//            [TestAspect]
//            public string Text { get; set; }
//        }

//        private class Target : TargetBase<Asset1>
//        {
//            [TestAspect]
//            public double Value { get; set; }
//        }

//        [Aspect(Scope.PerInstance)]
//        [Injection(typeof(TestAspect))]
//        private class TestAspect : Attribute
//        {
//            [Advice(Kind.Before)]
//            public void Before()
//            {
//                Console.WriteLine("Before");
//            }
//        }
//    }
//}
