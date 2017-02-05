using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class StaticTests
    {
        [TestMethod]
        public void Advices_InjectBeforeStaticMethod()
        {
            Checker.Passed = false;
            StaticTests_BeforeTarget.TestStaticMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Advices_InjectAroundStaticMethod()
        {
            Checker.Passed = false;

            int v = 2;
            object vv = v;

            StaticTests_AroundTarget.Do123((System.Int32)vv, new StringBuilder(), new object(), false, false);

            Assert.IsTrue(Checker.Passed);
        }

        public class StaticTests_AroundTarget
        {
            [Inject(typeof(StaticTests_AroundAspect1))]
            [Inject(typeof(StaticTests_AroundAspect2))] //fire first
            public static int Do123(int data, StringBuilder sb, object to, bool passed, bool passed2)
            {
                Checker.Passed = passed && passed2;

                var a = 1;
                var b = a + data;
                return b;
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class StaticTests_AroundAspect1
        {
            [Advice(Advice.Type.Around, Advice.Target.Method)]
            public object AroundMethod([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], true, arguments[4] });
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class StaticTests_AroundAspect2
        {
            [Advice(Advice.Type.Around, Advice.Target.Method)]
            public object AroundMethod([Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
                [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
            {
                return target(new object[] { arguments[0], arguments[1], arguments[2], arguments[3], true });
            }
        }

        [Inject(typeof(StaticTests_BeforeAspect))]
        internal class StaticTests_BeforeTarget
        {
            public static void TestStaticMethod()
            {
            }

            public void TestInstanceMethod()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class StaticTests_BeforeAspect
        {
            //Property
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod() { Checker.Passed = true; }
        }
    }
}