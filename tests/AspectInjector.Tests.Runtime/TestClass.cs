using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime
{
    internal static partial class TestClassWrapper<T1>
    {
        public partial class TestClass<T2>
        {
            public TestClass(
                int a1, StringBuilder a2, T1 a3, T2 a4,
                ref int ar1, ref StringBuilder ar2, ref T1 ar3, ref T2 ar4,
                out int ao1, out StringBuilder ao2, out T1 ao3, out T2 ao4
                )
            {
                TestLog.Write(Events.TestConstructorEnter);

                ao1 = ar1;
                ao2 = ar2;
                ao3 = ar3;
                ao4 = ar4;

                TestLog.Write(Events.TestConstructorExit);
            }

            public Tuple<int, StringBuilder, T1, T2, T3> TestMethod<T3>(
                int a1, StringBuilder a2, T1 a3, T2 a4, T3 a5,
                ref int ar1, ref StringBuilder ar2, ref T1 ar3, ref T2 ar4, ref T3 ar5,
                out int ao1, out StringBuilder ao2, out T1 ao3, out T2 ao4, out T3 ao5
                )
            {
                TestLog.Write(Events.TestMethodEnter);

                ao1 = ar1;
                ao2 = ar2;
                ao3 = ar3;
                ao4 = ar4;
                ao5 = ar5;

                TestLog.Write(Events.TestMethodExit);

                return new Tuple<int, StringBuilder, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            public IEnumerable<Tuple<int, StringBuilder, T1, T2, T3>> TestIteratorMethod<T3>(int a1, StringBuilder a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestIteratorMethodEnter);

                for (int i = 0; i < a1; i++)
                {
                    yield return new Tuple<int, StringBuilder, T1, T2, T3>(a1, a2, a3, a4, a5);
                }

                TestLog.Write(Events.TestIteratorMethodExit);
            }

            public async Task<Tuple<int, StringBuilder, T1, T2, T3>> TestAsyncMethod1<T3>(int a1, StringBuilder a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);

                return new Tuple<int, StringBuilder, T1, T2, T3>(a1, a2, a3, a4, a5);
            }

            public async Task TestAsyncMethod2<T3>(int a1, StringBuilder a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);
            }

            public async void TestAsyncMethod3<T3>(int a1, StringBuilder a2, T1 a3, T2 a4, T3 a5)
            {
                TestLog.Write(Events.TestAsyncMethodEnter);

                await Task.Delay(200);

                TestLog.Write(Events.TestAsyncMethodExit);
            }

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
        }
    }
}