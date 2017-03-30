//using AspectInjector.Broker;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.IO;

//namespace AspectInjector.Tests.Interfaces
//{
//    [TestClass]
//    public class GenericInterfacesTests
//    {
//        [Broker.Inject(typeof(Aspect))]
//        public class TestClass
//        {
//            public TestClass()
//            {
//            }
//        }

//        public interface ITestInterface<in TI, out TO>
//        {
//            TO Get { get; }

//            TI Set { set; }
//        }

//        [Mixin(typeof(ITestInterface<string, int>))]
//        [Broker.Aspect(Broker.Aspect.Scope.Global)]
//        public class Aspect : ITestInterface<string, int>
//        {
//            public int Get
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

//    [TestClass]
//    public class GenericInterfacesTests2
//    {
//        [TestMethod]
//        public void Interfaces_OpenGenericMethod()
//        {
//            var ti = (ITestInterface)new TestClass();
//            var r1 = ti.Get<string>("data");

//            Assert.AreEqual(r1, "datawashere");

//            var r2 = ti.Get<int>(1);

//            Assert.AreEqual(r2, 2);
//        }

//        [Broker.Inject(typeof(Aspect))]
//        public class TestClass
//        {
//            public TestClass()
//            {
//            }
//        }

//        public interface ITestInterface
//        {
//            TO Get<TO>(TO data);
//        }

//        [Mixin(typeof(ITestInterface))]
//        [Broker.Aspect(Broker.Aspect.Scope.Global)]
//        public class Aspect : ITestInterface
//        {
//            TO ITestInterface.Get<TO>(TO data)
//            {
//                if (data is string)
//                    return (TO)(object)(data.ToString() + "washere");

//                return (TO)(object)(((int)(object)data) + 1);
//            }
//        }
//    }

//    [TestClass]
//    public class GenericInterfacesTests3
//    {
//        [TestMethod]
//        public void Interfaces_OpenGenericMethodInClosedGenericType()
//        {
//            var data4 = "ref";

//            var ti = (IfaceWrapClass<string>.ITestInterface<StreamReader>)new TestClass();
//            var r1 = ti.Get<string>("data", "123", new StreamReader(new MemoryStream()), ref data4);

//            Assert.AreEqual(r1, "data123washereref");

//            var r2 = ti.Get<int>(1, "123", new StreamReader(new MemoryStream()), ref data4);

//            Assert.AreEqual(r2, "5");
//        }

//        [Broker.Inject(typeof(Aspect))]
//        public class TestClass
//        {
//            public TestClass()
//            {
//            }
//        }

//        public class IfaceWrapClass<TH>
//            where TH : class
//        {
//            public interface ITestInterface<T1> : ITestInterface2<TH, T1>
//                where T1 : TextReader
//            {
//            }

//            public interface ITestInterface2<T1, T2>
//                where T1 : TH
//            {
//                T1 Get<TO>(TO data, T1 data2, T2 data3, ref string data4);
//            }
//        }

//        [Mixin(typeof(IfaceWrapClass<string>.ITestInterface<StreamReader>))]
//        [Broker.Aspect(Broker.Aspect.Scope.Global)]
//        public class Aspect : IfaceWrapClass<string>.ITestInterface<StreamReader>
//        {
//            public string Get<TO>(TO data, string data2, StreamReader data3, ref string data4)
//            {
//                if (data is string)
//                    return (data.ToString() + data2 + "washere" + data4);

//                return (((int)(object)data) + 1 + data2.Length).ToString();
//            }
//        }
//    }
//}