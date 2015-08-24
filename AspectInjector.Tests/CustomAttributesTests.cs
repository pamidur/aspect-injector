using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class CustomAttributesTests
    {
        [TestMethod]
        public void Custom_Attributes_Pass_Routable_Values()
        {
            Checker.Passed = false;

            var a = new CustomAttributesTests_Target();
            a.Do();

            Assert.IsTrue(Checker.Passed);

            var b = new CustomAttributesTests_AspectAttribute("111") { Value = "olo" };
        }
    }

    [CustomAttributesTests_Aspect("TestHeader", Value = "ololo", data = 43)]
    public class CustomAttributesTests_Target
    {
        public void Do() { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    [CustomAspectDefinition(typeof(CustomAttributesTests_Aspect))]
    public class CustomAttributesTests_AspectAttribute : Attribute
    {
        public string Header { get; private set; }
        public string Value { get; set; }
        public int data = 42;

        public CustomAttributesTests_AspectAttribute(string header)
        {
            Header = header;
        }
    }

    public class CustomAttributesTests_Aspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.RoutableData)] object data)
        {
            var a = (data as CustomAttributesTests_AspectAttribute);

            Checker.Passed = a.Header == "TestHeader" && a.Value == "ololo" && a.data == 43;
        }
    }
}
