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
}