using AspectInjector.Broker;
using System;
using System.Reflection;
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

            a.Do1(new object(), 1, ref ref1, out out1, ref ref2, out out2, false, false);

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
            public int Do1(object data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                testOut = new object();
                testOutValue = 1;

                return 1;
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

        [Aspect(Aspect.Scope.Global)]
        [InjectionTrigger(typeof(AroundTests_Aspect1))]
        internal class AroundTests_Aspect1 : Attribute
        {
            [Advice(Advice.Kind.Around, Targets = Advice.Target.Method)]
            public object AroundMethod([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], true, arguments[7] });
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [InjectionTrigger(typeof(AroundTests_Aspect2))]
        internal class AroundTests_Aspect2 : Attribute
        {
            [Advice(Advice.Kind.Around, Targets = Advice.Target.Method)]
            public object AroundMethod([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], true });
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

        [Aspect(Aspect.Scope.Global)]
        [InjectionTrigger(typeof(AroundTests_ArgumentsModificationAspect))]
        internal class AroundTests_ArgumentsModificationAspect : Attribute
        {
            [Advice(Advice.Kind.Around, Targets = Advice.Target.Method)]
            public object AroundMethod(
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
                [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target
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