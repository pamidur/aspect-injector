using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace AspectInjector.Tests.AroundTests
{
    [TestClass]
    public class BasicAroundTests
    {
        [TestMethod]
        public void Aspect_Can_Wrap_Method_With_Struct_Result_Around()
        {
            Checker.Passed = false;

            var a = new TestClass();

            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            a.Do1(new object(), 1, ref ref1, out out1, ref ref2, out out2, false, false);

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Aspect_Can_Wrap_Method_Around()
        {
            Checker.Passed = false;

            var a = new TestClass();

            object ref1 = new object();
            object out1;
            int ref2 = 2;
            int out2;

            a.Do2(new object(), 1, ref ref1, out out1, ref ref2, out out2, false, false);

            Assert.IsTrue(Checker.Passed);
        }

        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            [Aspect(typeof(TestAspectImplementation2))] //fire first
            public object Do2(object data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                testOut = new object();
                testOutValue = 1;

                return new object();
            }

            [Aspect(typeof(TestAspectImplementation))]
            [Aspect(typeof(TestAspectImplementation2))] //fire first
            public int Do1(object data, int value, ref object testRef, out object testOut, ref int testRefValue, out int testOutValue, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                testOut = new object();
                testOutValue = 1;

                return 1;
            }
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], true, arguments[7] });
            }
        }

        public class TestAspectImplementation2
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], true });
            }
        }
    }
}