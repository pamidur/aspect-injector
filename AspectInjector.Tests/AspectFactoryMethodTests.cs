using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.AspectFactoryMethod
{
    [TestClass]
    public class AspectFactoryMethodTests
    {
        [TestMethod]
        public void Create_Aspect_Through_Factory_Method()
        {
            Checker.Passed = false;
            var test = new TestClass();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void Create_Aspect_Through_Factory_Method_With_Args()
        {
            Checker.Passed = false;
            var test = new TestClassWithArgs();
            Assert.IsTrue(Checker.Passed);
        }
    }

    //[Aspect(typeof(TestAspectWithArgs))]
    public class TestClassWithArgs
    {
    }

    public class TestAspectWithArgs
    {
        private object target;

        public TestAspectWithArgs(object target)
        {
            this.target = target;
        }

        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void TestAdvice([AdviceArgument(AdviceArgumentSource.Instance)] object target)
        {
            Checker.Passed = this.target == target;
        }

        [AspectFactory]
        public static TestAspectWithArgs Create([AdviceArgument(AdviceArgumentSource.Instance)] object target)
        {
            return new TestAspectWithArgs(target);
        }
    }

    [Aspect(typeof(TestAspect))]
    public class TestClass
    {
    }

    public class TestAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Constructor)]
        public void TestAdvice()
        {
        }

        [AspectFactory]
        public static TestAspect Create()
        {
            Checker.Passed = true;
            return new TestAspect();
        }
    }
}