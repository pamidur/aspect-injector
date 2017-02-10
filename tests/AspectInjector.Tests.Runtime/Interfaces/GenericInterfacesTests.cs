using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.Interfaces
{
    [TestClass]
    public class GenericInterfacesTests
    {
        [Broker.Inject(typeof(Aspect))]
        public class TestClass
        {
            public TestClass()
            {
                var a = 1;
            }
        }

        public interface ITestInterface<in TI, out TO>
        {
            TO Get { get; }

            TI Set { set; }
        }

        [Mixin(typeof(ITestInterface<string, int>))]
        [Broker.Aspect(Broker.Aspect.Scope.Global)]
        public class Aspect : ITestInterface<string, int>
        {
            public int Get
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Set
            {
                set
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    [TestClass]
    public class GenericInterfacesTests2
    {
        [TestMethod]
        public void Interfaces_OpenGenericMethod()
        {
            var ti = (ITestInterface)new TestClass();
            var r1 = ti.Get<string>("data");

            Assert.AreEqual(r1, "datawashere");

            var r2 = ti.Get<int>(1);

            Assert.AreEqual(r2, 2);
        }

        [Broker.Inject(typeof(Aspect))]
        public class TestClass
        {
            public TestClass()
            {
                var a = 1;
            }
        }

        public interface ITestInterface
        {
            TO Get<TO>(TO data);
        }

        [Mixin(typeof(ITestInterface))]
        [Broker.Aspect(Broker.Aspect.Scope.Global)]
        public class Aspect : ITestInterface
        {
            TO ITestInterface.Get<TO>(TO data)
            {
                if (data is string)
                    return (TO)(object)(data.ToString() + "washere");

                return (TO)(object)(((int)(object)data) + 1);
            }
        }
    }
}