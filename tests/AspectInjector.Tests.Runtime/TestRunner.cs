using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AspectInjector.Tests.Runtime
{
    public class TestAssets
    {
        public static int asset1 = 1;
        public static StringBuilder asset2 = new StringBuilder();
        public static short asset3 = 2;
        public static IDisposable asset4 = new MemoryStream();
        public static TextReader asset5 = new StreamReader((Stream)asset4);
    }

    public class TestRunner
    {
        private static readonly List<string> _staticCtorEvents;

        static TestRunner()
        {
            TestLog.Reset();

            var type = typeof(TestClassWrapper<short>.TestClass<IDisposable>);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            _staticCtorEvents = TestLog.Log.ToList();
            TestLog.Reset();
        }

        [TestInitialize]
        public void Init()
        {
            TestLog.Reset();
        }

        public List<string> GetConstructorArgsSequence(string prefix)
        {
            return new List<string> {
                $"{prefix}:{GetArgEvent(TestAssets.asset1)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset2)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset3)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset4)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset1)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset2)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset3)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset4)}",
                $"{prefix}:{GetArgEvent(default(int))}",
                $"{prefix}:{GetArgEvent(null)}",
                $"{prefix}:{GetArgEvent(default(short))}",
                $"{prefix}:{GetArgEvent(null)}",
            };
        }

        public List<string> GetMethodArgsSequence(string prefix)
        {
            return new List<string> {
                $"{prefix}:{GetArgEvent(TestAssets.asset1)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset2)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset3)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset4)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset5)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset1)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset2)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset3)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset4)}",
                $"{prefix}:{GetArgEvent(TestAssets.asset5)}",
                $"{prefix}:{GetArgEvent(default(int))}",
                $"{prefix}:{GetArgEvent(null)}",
                $"{prefix}:{GetArgEvent(default(short))}",
                $"{prefix}:{GetArgEvent(null)}",
                $"{prefix}:{GetArgEvent(null)}",
            };
        }

        private string GetArgEvent(object o)
        {
            if (o == null)
                return $"Arguments:null";
            return $"Arguments:{o.GetType().Name}:{o.ToString()}";
        }

        public void ExecConstructor()
        {
            int ao1;
            StringBuilder ao2;
            short ao3;
            IDisposable ao4;

            var test = new TestClassWrapper<short>.TestClass<IDisposable>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, ref TestAssets.asset1, ref TestAssets.asset2, ref TestAssets.asset3, ref TestAssets.asset4, out ao1, out ao2, out ao3, out ao4);

            Assert.AreEqual(TestAssets.asset1, ao1);
            Assert.AreEqual(TestAssets.asset2, ao2);
            Assert.AreEqual(TestAssets.asset3, ao3);
            Assert.AreEqual(TestAssets.asset4, ao4);
        }

        public void ExecStaticConstructor()
        {
            _staticCtorEvents.ForEach(TestLog.Write);
        }

        public void ExecMethod()
        {
            int ao1;
            StringBuilder ao2;
            short ao3;
            IDisposable ao4;
            TextReader ao5;

            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestMethod<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5, ref TestAssets.asset1, ref TestAssets.asset2, ref TestAssets.asset3, ref TestAssets.asset4, ref TestAssets.asset5, out ao1, out ao2, out ao3, out ao4, out ao5);

            Assert.AreEqual(TestAssets.asset1, ao1);
            Assert.AreEqual(TestAssets.asset2, ao2);
            Assert.AreEqual(TestAssets.asset3, ao3);
            Assert.AreEqual(TestAssets.asset4, ao4);
            Assert.AreEqual(TestAssets.asset5, ao5);

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecStaticMethod()
        {
            int ao1;
            StringBuilder ao2;
            short ao3;
            IDisposable ao4;
            TextReader ao5;

            var result = TestClassWrapper<short>.TestClass<IDisposable>.TestStaticMethod<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5, ref TestAssets.asset1, ref TestAssets.asset2, ref TestAssets.asset3, ref TestAssets.asset4, ref TestAssets.asset5, out ao1, out ao2, out ao3, out ao4, out ao5);

            Assert.AreEqual(TestAssets.asset1, ao1);
            Assert.AreEqual(TestAssets.asset2, ao2);
            Assert.AreEqual(TestAssets.asset3, ao3);
            Assert.AreEqual(TestAssets.asset4, ao4);
            Assert.AreEqual(TestAssets.asset5, ao5);

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecSetter()
        {
            new TestClassWrapper<short>.TestClass<IDisposable>().TestProperty = new Tuple<short, IDisposable>(TestAssets.asset3, TestAssets.asset4);
        }

        public void ExecStaticSetter()
        {
            TestClassWrapper<short>.TestClass<IDisposable>.TestStaticProperty = new Tuple<short, IDisposable>(TestAssets.asset3, TestAssets.asset4);
        }

        public void ExecGetter()
        {
            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestProperty;
        }

        public void ExecStaticGetter()
        {
            var result = TestClassWrapper<short>.TestClass<IDisposable>.TestStaticProperty;
        }

        public void ExecAdd()
        {
            new TestClassWrapper<short>.TestClass<IDisposable>().TestEvent += (s, e) => { };
        }

        public void ExecStaticAdd()
        {
            TestClassWrapper<short>.TestClass<IDisposable>.TestStaticEvent += (s, e) => { };
        }

        public void ExecRemove()
        {
            new TestClassWrapper<short>.TestClass<IDisposable>().TestEvent -= (s, e) => { };
        }

        public void ExecStaticRemove()
        {
            TestClassWrapper<short>.TestClass<IDisposable>.TestStaticEvent -= (s, e) => { };
        }

        public void ExecIteratorMethod()
        {
            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestIteratorMethod<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).First();

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecStaticIteratorMethod()
        {
            var result = TestClassWrapper<short>.TestClass<IDisposable>.TestStaticIteratorMethod<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).First();

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecAsyncTypedTaskMethod()
        {
            var result = new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod1<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).Result;

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecStaticAsyncTypedTaskMethod()
        {
            var result = TestClassWrapper<short>.TestClass<IDisposable>.TestStaticAsyncMethod1<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).Result;

            Assert.AreEqual(TestAssets.asset1, result.Item1);
            Assert.AreEqual(TestAssets.asset2, result.Item2);
            Assert.AreEqual(TestAssets.asset3, result.Item3);
            Assert.AreEqual(TestAssets.asset4, result.Item4);
            Assert.AreEqual(TestAssets.asset5, result.Item5);
        }

        public void ExecAsyncTaskMethod()
        {
            new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod2<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).Wait();
        }

        public void ExecStaticAsyncTaskMethod()
        {
            TestClassWrapper<short>.TestClass<IDisposable>.TestStaticAsyncMethod2<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5).Wait();
        }

        public void ExecAsyncVoidMethod()
        {
            new TestClassWrapper<short>.TestClass<IDisposable>().TestAsyncMethod3<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5);
        }

        public void ExecStaticAsyncVoidMethod()
        {
            TestClassWrapper<short>.TestClass<IDisposable>.TestStaticAsyncMethod3<TextReader>(TestAssets.asset1, TestAssets.asset2, TestAssets.asset3, TestAssets.asset4, TestAssets.asset5);
        }

        public void CheckSequence(IReadOnlyList<string> orderedEvents)
        {
            var logEvents = TestLog.Log.Where(e => orderedEvents.Contains(e));

            if (!logEvents.SequenceEqual(orderedEvents))
                Assert.Fail(string.Join(Environment.NewLine, logEvents));
        }
    }
}