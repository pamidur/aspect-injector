using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AroundTests
{
    [TestClass]
    internal class BasicAroundTests
    {
        [TestMethod]
        public void Aspect_Can_Wrap_Method_Around()
        {
            Checker.Passed = false;

            var a = new TestClass();

            Int32 v = 2;
            object vv = v;

            a.Do123((System.Int32)vv, new StringBuilder(), new object(), false, false);

            Assert.IsTrue(Checker.Passed);
        }

        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation))]
            [Aspect(typeof(TestAspectImplementation2))] //fire first
            public int Do123(int data, StringBuilder sb, object to, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                var a = 1;
                var b = a + data;
                return b;
            }
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], true, arguments[4] });
            }
        }

        public class TestAspectImplementation2
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod([AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], true });
            }
        }
    }
}