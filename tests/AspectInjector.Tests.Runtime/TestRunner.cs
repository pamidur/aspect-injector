using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public void ExecMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            int ao1;
            StringBuilder ao2;
            short ao3;
            IDisposable ao4;
            TextReader ao5;

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestMethod<TextReader>(a1, a2, a3, a4, a5, ref a1, ref a2, ref a3, ref a4, ref a5, out ao1, out ao2, out ao3, out ao4, out ao5);

            Assert.AreEqual(a1, ao1);
            Assert.AreEqual(a2, ao2);
            Assert.AreEqual(a3, ao3);
            Assert.AreEqual(a4, ao4);
            Assert.AreEqual(a5, ao5);

            Assert.AreEqual(a1, result.Item1);
            Assert.AreEqual(a2, result.Item2);
            Assert.AreEqual(a3, result.Item3);
            Assert.AreEqual(a4, result.Item4);
            Assert.AreEqual(a5, result.Item5);
        }

        public void ExecIteratorMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestIteratorMethod<TextReader>(a1, a2, a3, a4, a5).First();

            Assert.AreEqual(a1, result.Item1);
            Assert.AreEqual(a2, result.Item2);
            Assert.AreEqual(a3, result.Item3);
            Assert.AreEqual(a4, result.Item4);
            Assert.AreEqual(a5, result.Item5);
        }

        public void EnterIteratorMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestIteratorMethod<TextReader>(a1, a2, a3, a4, a5);
        }

        public void ExecAsyncTypedTaskMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod1<TextReader>(a1, a2, a3, a4, a5).Result;

            Assert.AreEqual(a1, result.Item1);
            Assert.AreEqual(a2, result.Item2);
            Assert.AreEqual(a3, result.Item3);
            Assert.AreEqual(a4, result.Item4);
            Assert.AreEqual(a5, result.Item5);
        }

        public void ExecAsyncTaskMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod2<TextReader>(a1, a2, a3, a4, a5).Wait();
        }

        public void EnterAsyncTypedTaskMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod1<TextReader>(a1, a2, a3, a4, a5);
        }

        public void EnterAsyncTaskMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod2<TextReader>(a1, a2, a3, a4, a5);
        }

        public void ExecAsyncVoidMethod()
        {
            var a1 = 1;
            var a2 = new StringBuilder();
            short a3 = 2;
            IDisposable a4 = new MemoryStream();
            TextReader a5 = new StreamReader((Stream)a4);

            new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod3<TextReader>(a1, a2, a3, a4, a5);
        }

        public void CheckSequence(IReadOnlyList<string> orderedEvents)
        {
            var logEvents = TestLog.Log.Where(e => orderedEvents.Contains(e));

            if (!logEvents.SequenceEqual(orderedEvents))
                Assert.Fail(string.Join(Environment.NewLine, logEvents));
        }
    }
}