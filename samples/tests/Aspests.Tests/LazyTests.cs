using Aspects.Lazy;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aspests.Tests
{
    public class LazyTests
    {
        [DebuggerDisplay("{DateTime.ToString(\"HH: mm:ss.fffffff\")}")]
        class ServiceA
        {
            public DateTime DateTime { get; set; }

            public string Name { get; set; }

            public ServiceA(DateTime dateTime, [CallerMemberName] string name = "")
            {
                DateTime = dateTime;
                Name = name;
            }
        }

        class TestClass
        {
            [Lazy]
            public ServiceA ServiceA => new ServiceA(DateTime.Now);


            [Lazy]
            public ServiceA ServiceB
            {
                get { return new ServiceA(DateTime.Now); }
            }

            [Lazy]
            public ServiceA ServiceB1 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB2 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB3 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB4 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB5 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB6 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB7 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB8 => new ServiceA(DateTime.Now);

            [Lazy]
            public ServiceA ServiceB9 => new ServiceA(DateTime.Now);

            public ServiceA NoLazyService => new ServiceA(DateTime.Now);
        }

        [Fact]
        public void GetOnlyProperty_NoLazy_ReturnDifferentEachTime()
        {
            var t = new TestClass();

            var first = t.NoLazyService.DateTime;
            var second = t.NoLazyService.DateTime;

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void LazyInitializeOnce_ReturnSameResultEachTime()
        {
            var t = new TestClass();

            var time = t.ServiceA.DateTime;
            Thread.Sleep(10);

            Assert.Equal(time, t.ServiceA.DateTime);
        }

        [Fact]
        public void LazyInitialize_PerProperty_Test()
        {
            var t = new TestClass();

            var timeA = t.ServiceA.DateTime;
            var timeB = t.ServiceB.DateTime;

            Assert.NotEqual(timeA, timeB);
        }

        [Fact]
        public void LazyInitialize_Concurrent_Test()
        {
            var t = new TestClass();

            var tasks = new Task<ServiceA>[100];
            for (int i = 0; i < tasks.Length; i++)
            {
                Func<TestClass, ServiceA> func;

                switch (i % 10)
                {
                    case 1:
                        func = (o) => o.ServiceB1;
                        break;
                    case 2:
                        func = (o) => o.ServiceB2;
                        break;
                    case 3:
                        func = (o) => o.ServiceB3;
                        break;
                    case 4:
                        func = (o) => o.ServiceB4;
                        break;
                    case 5:
                        func = (o) => o.ServiceB5;
                        break;
                    case 6:
                        func = (o) => o.ServiceB6;
                        break;
                    case 7:
                        func = (o) => o.ServiceB7;
                        break;
                    case 8:
                        func = (o) => o.ServiceB8;
                        break;
                    case 9:
                        func = (o) => o.ServiceB9;
                        break;
                    default:
                        func = (o) => o.ServiceB1;
                        break;
                }

                tasks[i] = new Task<ServiceA>(() => func(t));
            }

            Parallel.ForEach(tasks, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, task => task.Start());
            Task.WaitAll(tasks);

            var result = tasks.GroupBy(o => o.Result.DateTime);
            Assert.True(result.Count() == 9);
        }

        #region Static Property

        class StaticTestA
        {
            [Lazy]
            public static ServiceA ServiceA => new ServiceA(DateTime.Now);

            public static string S { get; set; } = "";

            public static string SMethod => "";
        }

        class StaticTestB
        {
            [Lazy]
            public static ServiceA ServiceA => new ServiceA(DateTime.Now);
        }

        [Fact]
        public void LazyInitializeOnce_StaticProperty_ReturnSameResultEachTime()
        {
            var first = StaticTestA.ServiceA;
            var second = StaticTestA.ServiceA;

            Assert.Equal(first, second);
        }

        [Fact]
        public void LazyInitialize_PerStaticProperty_Test()
        {
            var testA = StaticTestA.ServiceA;
            var testB = StaticTestB.ServiceA;

            Assert.NotEqual(testA, testB);
        }

        #endregion

        #region Generic class

        class GenericTestClass<T>
        {
            [Lazy]
            public static ServiceA ServiceA => new ServiceA(DateTime.Now);

            [Lazy]
            public static T ServiceB => default;
        }

        [Fact]
        public void LazyInitialize_GenericClass_PerStaticProperty_Test()
        {
            var testA = GenericTestClass<int>.ServiceA;
            var testB = GenericTestClass<string>.ServiceA;

            Assert.NotEqual(testA, testB);
        }

        #endregion
    }
}
