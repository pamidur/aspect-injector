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
    public class PartialMembersInterfacesTests
    {
        [Aspect(typeof(Aspect))]
        public class TestClass
        {
        }

        public interface ITestInterface
        {
            string Get { get; }
            string Set { set; }
        }

        [AdviceInterfaceProxy(typeof(ITestInterface))]
        public class Aspect : ITestInterface
        {
            public string Get
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