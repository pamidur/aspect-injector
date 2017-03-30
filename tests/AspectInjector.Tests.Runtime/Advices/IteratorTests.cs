using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Advices
{
    [TestClass]
    public class IteratorTests
    {
        [TestMethod]
        public void Advices_InjectAfterIteratorMethod()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get("test"))
            {
                Assert.IsFalse(Checker.Passed);
                d.Equals('a');
            }

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterIteratorMethod_WithArgs()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get1("test"))
            {
                Assert.IsFalse(Checker.Passed);
                d.Equals('a');
            }

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAfterIteratorMethod2()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get2())
            {
                Assert.IsFalse(Checker.Passed);
                d.Equals('a');
            }

            Assert.IsTrue(Checker.Passed);
        }

        public class TargetClass
        {
            [Inject(typeof(TestAspect))]
            public IEnumerable<char> Get(string input)
            {
                foreach (var c in input)
                    yield return c;
            }

            [Inject(typeof(TestArgsAspect))]
            public IEnumerable<char> Get1(string input)
            {
                input = "ololo";
                foreach (var c in input)
                    yield return c;
            }

            [Inject(typeof(TestAspect))]
            public IEnumerable<char> Get2()
            {
                yield return 'a';
                yield return 'b';
                yield return 'c';
            }
        }

        public class TestComplexClass<T1>
        {
            [Inject(typeof(TestAspect))]
            public IEnumerable<Tuple<T1, T2>> Get2<T2>(
                int val, T1 t1, T2 t2, object o
                )
            {
                yield return null;
            }

            [Inject(typeof(TestAspect))]
            public IEnumerable<Tuple<T1>> Get3(int val, T1 t1, object o)
            {
                yield return null;
            }
        }

        [Aspect(Aspect.Scope.PerInstance)]
        public class TestAspect
        {
            [Advice(Advice.Type.After, Advice.Target.Method)]
            public void After()
            {
                Checker.Passed = true;
            }
        }

        [Aspect(Aspect.Scope.PerInstance)]
        public class TestArgsAspect
        {
            [Advice(Advice.Type.After, Advice.Target.Method)]
            public void After(
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
                [Advice.Argument(Advice.Argument.Source.ReturnValue)] object res
                )
            {
                Checker.Passed = args[0].ToString() == "ololo" && res is IEnumerable;
            }
        }
    }
}