using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime
{
    public class TestRunner
    {
        [TestInitialize]
        public void Init()
        {
            TestLog.Reset();
        }

        public void ExecConstructor()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();

            int ao1;
            StringBuilder ao2;
            short ao3;
            IDisposable ao4;

            var test = new TestClassWrapper<short>.TestClass<IDisposable>(a1, a2, a3, a4, ref a1, ref a2, ref a3, ref a4, out ao1, out ao2, out ao3, out ao4);

            Assert.AreEqual(a1, ao1);
            Assert.AreEqual(a2, ao2);
            Assert.AreEqual(a3, ao3);
            Assert.AreEqual(a4, ao4);
        }

        public void CheckSequence(IReadOnlyList<string> orderedEvents)
        {
            var logEvents = TestLog.Log.Intersect(orderedEvents).Distinct();

            if (!logEvents.SequenceEqual(orderedEvents))
                Assert.Fail(string.Join(Environment.NewLine, logEvents));
        }
    }
}