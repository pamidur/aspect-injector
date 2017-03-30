using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.General
{
    [TestClass]
    public class AspectFactoryTests
    {
        [TestMethod]
        public void General_AspectFactory_CreateAspect()
        {
            Checker.Passed = false;
            var test = new AspectFactoryTests_Target();
            Assert.IsTrue(Checker.Passed);
        }
    }

    [Inject(typeof(AspectFactoryTests_Aspect))]
    public class AspectFactoryTests_Target
    {
    }

    [Aspect(Aspect.Scope.PerInstance, Factory = typeof(AspectFactory))]
    public class AspectFactoryTests_Aspect
    {
        private static object aaa;

        [Advice(Advice.Type.After, Advice.Target.Constructor)]
        public void TestMethod()
        {
        }

        private static void ololo()
        {
            aaa = AspectFactory.GetInstance(typeof(AspectFactoryTests_Aspect));
        }
    }

    public class AspectFactory
    {
        public static object GetInstance(Type aspectType)
        {
            Checker.Passed = true;
            return Activator.CreateInstance(aspectType);
        }
    }
}