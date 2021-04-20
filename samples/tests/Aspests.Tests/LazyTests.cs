using Aspects.Lazy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Aspests.Tests
{
    public class LazyTests
    {
        class ServiceA
        {
            public DateTime DateTime { get; set; }

            public ServiceA(DateTime dateTime)
            {
                DateTime = dateTime;
            }
        }

        class TestClass
        {
            [Lazy]
            public ServiceA ServiceA => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB => new ServiceA(DateTime.Now);

            public string ServiceC => "ServiceC";
        }

        [Fact]
        public void Lazy_Initialize_Once_OneProperty_Test()
        {
            var t = new TestClass();

            var time = t.ServiceA.DateTime;
            Thread.Sleep(10);

            Assert.Equal(time, t.ServiceA.DateTime);
        }

        [Fact]
        public void Lazy_Initialize_PerProperty_Test()
        {
            var t = new TestClass();

            var timeA = t.ServiceA.DateTime;
            Thread.Sleep(10);
            var timeB = t.ServiceB.DateTime;

            Assert.NotEqual(timeA, timeB);
        }
    }
}
