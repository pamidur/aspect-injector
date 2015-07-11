using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.RoutableDataTests
{
    [TestClass]
    public class RoutableDataTests
    {
        [TestMethod]
        public void Routable_Data_Is_Passed_To_Aspect()
        {
            Checker.Passed = false;

            new TestClass().Do();

            Assert.IsTrue(Checker.Passed);
        }
    }

    [Aspect(typeof(TestAspect), RoutableData = new object[] { StringSplitOptions.None, (short)12, true, new object[] { new string[] { "fgf" }, new int[] { } }, new int[] { } })]
    public class TestClass
    {
        public void Do() { }
    }

    public class TestAspect
    {
        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void AfterMethod([AdviceArgument(AdviceArgumentSource.RoutableData)] object data)
        {
            var data012 = (object[])data;
            var data3 = (object[])data012[3];

            Checker.Passed =
                ((StringSplitOptions)data012[0]) == StringSplitOptions.None &&
                ((short)data012[1]) == (short)12 &&
                ((bool)data012[2]) == true &&
                data012[4] is int[] &&
                data3[0] is string[] &&
                (data3[0] as string[])[0] == "fgf" &&
                data3[1] is int[];
        }
    }
}
