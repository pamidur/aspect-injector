using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Interfaces
{
    [TestClass]
    public class GenericInterfacesTests
    {
        [Broker.Cut(typeof(Aspect))]
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