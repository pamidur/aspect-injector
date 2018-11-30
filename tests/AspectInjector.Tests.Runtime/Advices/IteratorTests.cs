using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using Xunit;

namespace AspectInjector.Tests.Runtime.Advices
{

    public class IteratorTests
    {
        [Fact]
        public void Advices_InjectAfterIteratorMethod()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get("test"))
            {
                Assert.False(Checker.Passed);
                d.Equals('a');
            }

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterIteratorMethod_WithArgs()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get1("test"))
            {
                Assert.False(Checker.Passed);
                d.Equals('a');
            }

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void Advices_InjectAfterIteratorMethod2()
        {
            Checker.Passed = false;

            var a = new TargetClass();

            foreach (var d in a.Get2())
            {
                Assert.False(Checker.Passed);
                d.Equals('a');
            }

            Assert.True(Checker.Passed);
        }

        public class TargetClass
        {
            [TestAspect]
            public IEnumerable<char> Get(string input)
            {
                foreach (var c in input)
                    yield return c;
            }

            [TestArgsAspect]
            public IEnumerable<char> Get1(string input)
            {
                input = "ololo";
                foreach (var c in input)
                    yield return c;
            }

            [TestAspect]
            public IEnumerable<char> Get2()
            {
                yield return 'a';
                yield return 'b';
                yield return 'c';
            }
        }

        public class TestComplexClass<T1>
        {
            [TestAspect]
            public IEnumerable<Tuple<T1, T2>> Get2<T2>(
                int val, T1 t1, T2 t2, object o
                )
            {
                yield return null;
            }

            [TestAspect]
            public IEnumerable<Tuple<T1>> Get3(int val, T1 t1, object o)
            {
                yield return null;
            }
        }

        [Aspect(Aspect.Scope.PerInstance)]
        [Injection(typeof(TestAspect))]
        public class TestAspect : Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void After()
            {
                Checker.Passed = true;
            }
        }

        [Aspect(Aspect.Scope.PerInstance)]
        [Injection(typeof(TestArgsAspect))]
        public class TestArgsAspect : Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void After(
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
                [Advice.Argument(Advice.Argument.Source.ReturnValue)] object res
                )
            {
                Checker.Passed = true;
            }
        }
    }
}