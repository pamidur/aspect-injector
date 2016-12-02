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
        [Aspect(typeof(Aspect<int>))]
        public class TestClass
        {
        }

        public interface ITestInterface<in TI, out TO>
        {
            TO Get { get; }
            TI Set { set; }
        }

        [AdviceInterfaceProxy(typeof(ITestInterface<string, int>))]
        public class Aspect<T> : ITestInterface<string, T>
        {
            public T Get
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