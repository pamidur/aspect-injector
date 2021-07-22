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

        interface IService
        {

        }

        class SerA : IService
        {

        }

        class SerB : IService
        {

        }

        class ServiceFactory
        {
            private static int options = 0;

            public IService CreateService()
            {
                options++;

                if (options == 1)
                {
                    return new SerA();
                }
                else
                {
                    return new SerB();
                }
            }
        }

        class GenericTestClass<T> where T : IService
        {
            [Lazy]
            public static ServiceA ServiceA => new ServiceA(DateTime.Now);

            [Lazy]
            public static T ServiceFromFactory => (T)new ServiceFactory().CreateService();
        }

        [Fact]
        public void LazyInitialize_GenericClass_StaticProperty_InitPerClass()
        {
            var testA = GenericTestClass<SerA>.ServiceA;
            var testB = GenericTestClass<SerB>.ServiceA;

            Assert.NotEqual(testA, testB);
        }

        [Fact]
        public void LazyInitialize_GenericProperty_InitEachType()
        {
            var serA = GenericTestClass<SerA>.ServiceFromFactory;
            var serB = GenericTestClass<SerB>.ServiceFromFactory;

            Assert.False(Object.Equals(serA, serB));

            var twice = GenericTestClass<SerA>.ServiceFromFactory;
            Assert.Equal(serA, twice);
        }

        #endregion
    }
}
