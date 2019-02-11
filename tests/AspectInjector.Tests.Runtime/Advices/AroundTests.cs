using AspectInjector.Broker;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace AspectInjector.Tests.Advices
{
    public class AroundTests
    {
        [Fact]
        public void Advices_InjectAroundMethod_StructResult()
        {
            Checker.Passed = false;

            var a = new AroundTests_Target();

            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;
            var str = "";
            var dt = DateTime.Now;
            var arr = new[] {1,2,3 };

            var sb = new StringBuilder();

            a.Do1<StringBuilder>(new object(), 1, str, ref str, dt, ref dt, sb, ref sb, arr, ref arr, ref ref1, out out1, ref ref2, out out2, false, false);

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAroundMethod()
        {
            Checker.Passed = false;

            var a = new AroundTests_Target();

            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            a.Do2(new object(), 1, ref ref1, out out1, ref ref2, out out2, false, false);

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAroundMethod_ModifyArguments()
        {
            var i = 1;

            Checker.Passed = true;
            new AroundTests_ArgumentsModificationTarget().Fact(ref i);
            Assert.True(Checker.Passed);
        }

        

        internal class AroundTests_Target
        {
            [AroundTests_Aspect1]
            [AroundTests_Aspect2] //fire first
            public object Do2(object data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                testOut = new object();
                testOutValue = 1;

                return new object();
            }

            [AroundTests_Aspect1]
            [AroundTests_Aspect2] //fire first
            public T Do1<T>(object data, int value, string str, ref string rstr, DateTime dt, ref DateTime rdt, T g, ref T rg, int[] arr, ref int[] rarr,  ref object testRef, out object testOut, ref int testRefValue, out int testOutValue, bool passed, bool passed2)
                where T : ISerializable
            {
                var test = new object[] { dt, rdt,arr,rarr,g,rg };

                dt = (DateTime)test[0];
                rdt = (DateTime)test[1];
                arr = (int[])test[2];
                rarr = (int[])test[3];
                g = (T)test[4];
                rg = (T)test[5];

                Checker.Passed = passed && passed2;

                testOut = new object();
                testOutValue = 1;

                return g;
            }

            [AroundTests_Aspect1]
            public object Object(object data)
            {
                Checker.Passed = true;
                return new object();
            }

            [AroundTests_Aspect1]
            public object ObjectRef(ref object data)
            {
                Checker.Passed = true;
                return new object();
            }

            [AroundTests_Aspect1]
            public object ObjectOut(out object data)
            {
                Checker.Passed = true;
                return data = new object();
            }

            [AroundTests_Aspect1]
            public object Value(int data)
            {
                Checker.Passed = true;
                return new object();
            }

            [AroundTests_Aspect1]
            public object ValueRef(ref int data)
            {
                Checker.Passed = true;

                var a = new object[] { 1 };
                data = (int)a[0];

                return new object();
            }

            [AroundTests_Aspect1]
            public object ValueOut(out int data)
            {
                Checker.Passed = true;
                data = 1;
                return new object();
            }

            [AroundTests_Aspect1]
            public object ValueBoxed(Int32 data)
            {
                Checker.Passed = true;
                return new object();
            }

            [AroundTests_Aspect1]
            public object TypedObjectRef(ref StrongNameKeyPair data)
            {
                Checker.Passed = true;

                var a = new object[] { default(StrongNameKeyPair) };
                data = (StrongNameKeyPair)a[0];

                return new object();
            }

            [AroundTests_Aspect1]
            public T GenericRef<T>(ref T data)
            {
                Checker.Passed = true;

                var a = new object[] { default(T) };
                data = (T)a[0];

                return data;
            }

            [AroundTests_Aspect1]
            public T GenericOut<T>(out T data)
            {
                Checker.Passed = true;
                data = default(T);
                return data;
            }

            [AroundTests_Aspect1]
            public T Generic<T>(T data)
            {
                Checker.Passed = true;
                data = default(T);
                return data;
            }
        }


        [Aspect(Scope.Global)]
        [Injection(typeof(AroundTests_Simple))]
        internal class AroundTests_Simple : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object AroundMethod([Argument(Source.Target)] Func<object[], object> target,
                [Argument(Source.Arguments)] object[] arguments)
            {
                return target(arguments);
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(AroundTests_Aspect1))]
        internal class AroundTests_Aspect1 : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object AroundMethod([Argument(Source.Target)] Func<object[], object> target,
                [Argument(Source.Arguments)] object[] arguments)
            {
                return target(arguments.Take(arguments.Length - 2).Concat(new object[] { true, arguments.Last() }).ToArray());
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(AroundTests_Aspect2))]
        internal class AroundTests_Aspect2 : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object AroundMethod([Argument(Source.Target)] Func<object[], object> target,
                [Argument(Source.Arguments)] object[] arguments)
            {
                return target(arguments.Take(arguments.Length - 1).Concat(new object[] { true }).ToArray());
            }
        }

        internal class AroundTests_ArgumentsModificationTarget
        {
            [AroundTests_ArgumentsModificationAspect]
            public void Fact(ref int i)
            {
                if (i == 2)
                    i = 3;
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(AroundTests_ArgumentsModificationAspect))]
        internal class AroundTests_ArgumentsModificationAspect : Attribute
        {
            [Advice(Kind.Around, Targets = Target.Method)]
            public object AroundMethod(
                [Argument(Source.Arguments)] object[] args,
                [Argument(Source.Target)] Func<object[], object> target
                )
            {
                args[0] = 2;

                var result = target(args);

                Checker.Passed = (int)args[0] == 3;

                return result;
            }
        }
    }
}