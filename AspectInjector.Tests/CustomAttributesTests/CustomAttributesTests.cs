using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.CustomAttributesTests
{
    [TestClass]
    public class TestCustomAttributes
    {
        [TestMethod]
        public void Custom_Attributes_Pass_Routable_Values()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Do();

            Assert.IsTrue(Checker.Passed);

            var b = new TestAspectAttribute("111") { Value = "olo" };
        }
    }

    [TestAspect("TestHeader", Value = "ololo", data = 43)]
    public class TestClass
    {
        public void Do()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    [AspectDefinition(typeof(TestAspectImplementation))]
    public class TestAspectAttribute : Attribute
    {
        public string Header { get; private set; }
        public string Value { get; set; }
        public int data = 42;

        public TestAspectAttribute(string header)
        {
            Header = header;
        }
    }

    public class TestAspectImplementation
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.RoutableData)] object[] data)
        {
            var a = (data[0] as TestAspectAttribute);

            Checker.Passed = a.Header == "TestHeader" && a.Value == "ololo" && a.data == 43;
        }
    }
}