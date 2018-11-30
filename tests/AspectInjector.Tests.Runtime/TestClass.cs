using AspectInjector.Broker;
using AspectInjector.Tests.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime
{
    internal static partial class TestClassWrapper<T1>
    {
        public partial class TestClass<T2>
        {
            public TestClass()
            {
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public TestClass(
                int a1, Asset1 a2, T1 a3, T2 a4,
                ref int ar1, ref Asset1 ar2, ref T1 ar3, ref T2 ar4,
                out int ao1, out Asset1 ao2, out T1 ao3, out T2 ao4
                )
            {
                TestLog.Write(Events.TestConstructorEnter);

                ao1 = ar1;
                ao2 = ar2;
                ao3 = ar3;
                ao4 = ar4;

                TestLog.Write(Events.TestConstructorExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            static TestClass()
            {
                TestLog.Write(Events.TestStaticConstructorEnter);

                TestLog.Write(Events.TestStaticConstructorExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public Tuple<int, Asset1, T1, T2, T3> Fact<T3>(
                int a1, Asset1 a2, T1 a3, T2 a4, T3 a5,
                ref int ar1, ref Asset1 ar2, ref T1 ar3, ref T2 ar4, ref T3 ar5,
                out int ao1, out Asset1 ao2, out T1 ao3, out T2 ao4, out T3 ao5
                )
            {
                TestLog.Write(Events.FactEnter);

                ao1 = ar1;
                ao2 = ar2;
                ao3 = ar3;
                ao4 = ar4;
                ao5 = ar5;

                TestLog.Write(Events.FactExit);

                return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static Tuple<int, Asset1, T1, T2, T3> TestStaticMethod<T3>(
                int a1, Asset1 a2, T1 a3, T2 a4, T3 a5,
                ref int ar1, ref Asset1 ar2, ref T1 ar3, ref T2 ar4, ref T3 ar5,
                out int ao1, out Asset1 ao2, out T1 ao3, out T2 ao4, out T3 ao5
                )
            {
                TestLog.Write(Events.TestStaticMethodEnter);

                ao1 = ar1;
                ao2 = ar2;
                ao3 = ar3;
                ao4 = ar4;
                ao5 = ar5;

                TestLog.Write(Events.TestStaticMethodExit);

                return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public IEnumerable<Tuple<int, Asset1, T1, T2, T3>> TestIteratorMethod<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestIteratorMethodEnter);

                for (int i = 0; i < a1; i++)
                {
                    yield return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
                }

                TestLog.Write(Events.TestIteratorMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static IEnumerable<Tuple<int, Asset1, T1, T2, T3>> TestStaticIteratorMethod<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestStaticIteratorMethodEnter);

                for (int i = 0; i < a1; i++)
                {
                    yield return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
                }

                TestLog.Write(Events.TestStaticIteratorMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public async Task<Tuple<int, Asset1, T1, T2, T3>> TestAsyncMethod1<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);

                return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static async Task<Tuple<int, Asset1, T1, T2, T3>> TestStaticAsyncMethod1<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestStaticAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestStaticAsyncMethodExit);

                return new Tuple<int, Asset1, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public async Task TestAsyncMethod2<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static async Task TestStaticAsyncMethod2<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestStaticAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestStaticAsyncMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public async void TestAsyncMethod3<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static async void TestStaticAsyncMethod3<T3>(int a1, Asset1 a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestStaticAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestStaticAsyncMethodExit);
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public Tuple<T1, T2> TestProperty
            {
                get
                {
                    TestLog.Write(Events.TestPropertyGetterEnter);

                    TestLog.Write(Events.TestPropertyGetterExit);
                    return null;
                }

                set
                {
                    TestLog.Write(Events.TestPropertySetterEnter);

                    TestLog.Write(Events.TestPropertySetterExit);
                }
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static Tuple<T1, T2> TestStaticProperty
            {
                get
                {
                    TestLog.Write(Events.TestStaticPropertyGetterEnter);

                    TestLog.Write(Events.TestStaticPropertyGetterExit);
                    return null;
                }

                set
                {
                    TestLog.Write(Events.TestStaticPropertySetterEnter);

                    TestLog.Write(Events.TestStaticPropertySetterExit);
                }
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public event EventHandler<Tuple<T1, T2>> TestEvent
            {
                add
                {
                    TestLog.Write(Events.TestEventAddEnter);

                    TestLog.Write(Events.TestEventAddExit);
                }

                remove
                {
                    TestLog.Write(Events.TestEventRemoveEnter);

                    TestLog.Write(Events.TestEventRemoveExit);
                }
            }

            [InjectInstanceAspect]
            [InjectGlobalAspect]
            public static event EventHandler<Tuple<T1, T2>> TestStaticEvent
            {
                add
                {
                    TestLog.Write(Events.TestStaticEventAddEnter);

                    TestLog.Write(Events.TestStaticEventAddExit);
                }

                remove
                {
                    TestLog.Write(Events.TestStaticEventRemoveEnter);

                    TestLog.Write(Events.TestStaticEventRemoveExit);
                }
            }
        }
    }
}