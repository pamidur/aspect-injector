//using AspectInjector.Broker;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;

//namespace AspectInjector.Tests.Interfaces
//{
//    [TestClass]
//    public class PartialMembersInterfacesTests
//    {
//        [Broker.Inject(typeof(Aspect))]
//        public class TestClass
//        {
//        }

//        public interface ITestInterface
//        {
//            string Get { get; }

//            string Set { set; }
//        }

//        [Mixin(typeof(ITestInterface))]
//        [Broker.Aspect(Broker.Aspect.Scope.Global)]
//        public class Aspect : ITestInterface
//        {
//            public string Get
//            {
//                get
//                {
//                    throw new NotImplementedException();
//                }
//            }

//            public string Set
//            {
//                set
//                {
//                    throw new NotImplementedException();
//                }
//            }
//        }
//    }
//}