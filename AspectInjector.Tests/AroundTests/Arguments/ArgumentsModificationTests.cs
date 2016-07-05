using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.AroundTests.Arguments
{
    [TestClass]
    public class ArgumentsModificationTests
    {
        [TestMethod]
        public void AdviceArguments_Arguments_Can_Be_Modified()
        {
            var i = 1;

            Checker.Passed = true;
            new TargetClass().TestMethod(ref i);
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod(ref int i)
            {
                if (i == 2)
                    i = 3;
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object AroundMethod(
                [AdviceArgument(AdviceArgumentSource.Arguments)] object[] args,
                [AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target
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