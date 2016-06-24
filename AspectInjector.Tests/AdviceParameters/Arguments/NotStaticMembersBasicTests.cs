using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AdviceParameters.Arguments
{
    [TestClass]
    public class NotStaticMembersBasicTests
    {
        [TestMethod]
        public void AdviceArguments_Arguments_InjectedCorrectly()
        {
            var obj = new object();
            object outObj;
            var val = 1;
            int outVal;

            Checker.Passed = false;
            new TargetClass().TestMethod(obj, ref obj, out outObj, val, ref val, out outVal);
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut)
            {
                valueOut = 1;
                objOut = new object();
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Arguments)] object[] args)
            {
                Checker.Passed =
                    args[0] != null && args[1] != null && args[2] == null &&
                    (int)args[3] == 1 && (int)args[4] == 1 && (int)args[5] == 0;
            }
        }
    }
}