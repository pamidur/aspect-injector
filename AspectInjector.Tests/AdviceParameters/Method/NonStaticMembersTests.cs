using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace AspectInjector.Tests.AdviceParameters.Method
{
    [TestClass]
    public class NonStaticMembersTests
    {
        [TestMethod]
        public void AdviceArguments_Method_Can_Be_Injected()
        {
            Checker.Passed = false;
            new TargetClass().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_of_Constructor_Can_Be_Injected()
        {
            Checker.Passed = false;
            var temp = new TargetClass2();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_Can_Be_Injected_With_Conctructors_Chain()
        {
            Checker.Passed = false;
            new TargetClass1().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        internal class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod()
            {
            }
        }

        internal class TargetClass1
        {
            public TargetClass1()
            {
                
            }

            public TargetClass1(int a) :this()
            {
                
            }

            public TargetClass1(int a, int b) : this(a)
            {

            }


            [Aspect(typeof(AspectImplementation))]
            public void TestMethod()
            {
            }
        }

        internal class TargetClass2
        {
            [Aspect(typeof(AspectImplementation))]
            public TargetClass2()
            {
            }
        }

        internal class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method | InjectionTargets.Constructor)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Method)] MethodBase method)
            {
                Checker.Passed = method != null;
            }
        }
    }
}