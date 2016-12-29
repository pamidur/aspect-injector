using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.General
{
    [TestClass]
    public class CustomAttributesTests
    {
        [TestMethod]
        public void General_CustomAttributes_PassRoutableValues()
        {
            Checker.Passed = false;

            var a = new CustomAttributesTests_Target();
            a.Do();

            Assert.IsTrue(Checker.Passed);

            var b = new CustomAttributesTestsAttribute("111") { Value = "olo" };
        }

        [TestMethod]
        public void General_CustomAttributes_Multiple()
        {
            Checker.Passed = false;

            var a = new CustomAttributesTests_MultipleTarget();
            a.Do123();

            Assert.IsTrue(Checker.Passed);
        }
    }

    [CustomAttributesTests("TestHeader", Value = "ololo", data = 43)]
    internal class CustomAttributesTests_Target
    {
        public void Do()
        {
        }
    }

    [CustomAttributesTests_Multiple1]
    internal class CustomAttributesTests_MultipleTarget
    {
        [CustomAttributesTests_Multiple2]
        public void Do123()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    [IncutSpecification(typeof(CustomAttributesTests_Aspect))]
    internal class CustomAttributesTestsAttribute : Attribute
    {
        public string Header { get; private set; }

        public string Value { get; set; }

        public int data = 42;

        public CustomAttributesTestsAttribute(string header)
        {
            Header = header;
        }
    }

    public class CustomAttributesTests_Aspect
    {
        [Advice(Advice.Type.After, Advice.Target.Method)]
        public void AfterMethod([AdviceArgument(AdviceArgument.Source.RoutableData)] Attribute[] data)
        {
            var a = (data[0] as CustomAttributesTestsAttribute);

            Checker.Passed = a.Header == "TestHeader" && a.Value == "ololo" && a.data == 43;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    [IncutSpecification(typeof(CustomAttributesTests_MultipleAspect), NameFilter = "Do")]
    public class CustomAttributesTests_Multiple1Attribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = true)]
    [IncutSpecification(typeof(CustomAttributesTests_MultipleAspect))]
    public class CustomAttributesTests_Multiple2Attribute : Attribute
    {
    }

    public class CustomAttributesTests_MultipleAspect
    {
        [Advice(Advice.Type.After, Advice.Target.Method)]
        public void AfterMethod([AdviceArgument(AdviceArgument.Source.RoutableData)] Attribute[] data)
        {
            Checker.Passed = data.Length == 2
                && data[0] is CustomAttributesTests_Multiple1Attribute
                && data[1] is CustomAttributesTests_Multiple2Attribute;
        }
    }
}