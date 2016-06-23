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
    public class MethodParameterTests
    {
        [TestMethod]
        public void AdviceArguments_Method_Is_Correct_On_Around()
        {
            Checker.Passed = false;
            new TargetClass().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod()
            {
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Around, InjectionTargets.Method)]
            public object BeforeMethod([AdviceArgument(AdviceArgumentSource.Method)] MethodBase method)
            {
                Checker.Passed = method.Name == "TestMethod";
                return null;
            }
        }
    }
}