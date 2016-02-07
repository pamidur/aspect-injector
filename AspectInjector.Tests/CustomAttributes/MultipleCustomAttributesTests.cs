using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.CustomAttributes
{
    [TestClass]
    public class TestMultipleCustomAttributes
    {
        [TestMethod]
        public void Multiple_Custom_Attributes_Are_Passed()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Do123();

            Assert.IsTrue(Checker.Passed);
        }

        [TestAspect]
        public class TestClass
        {
            [TestAspect2]
            public void Do123()
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
        [AspectDefinition(typeof(TestAspectImplementation), NameFilter = "Do")]
        public class TestAspectAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
        [AspectDefinition(typeof(TestAspectImplementation))]
        public class TestAspect2Attribute : Attribute
        {
        }

        public class TestAspectImplementation
        {
            [Advice(InjectionPoints.After, InjectionTargets.Method)]
            public void AfterMethod([AdviceArgument(AdviceArgumentSource.RoutableData)] Attribute[] data)
            {
                Checker.Passed = data.Length == 2 && data[0] is TestAspectAttribute && data[1] is TestAspect2Attribute;
            }
        }
    }
}