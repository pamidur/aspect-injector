using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void Advices_InjectAfterMethod_NameFilter()
        {
            Checker.Passed = false;

            var a = new FilterTests_Target();
            a.Do123();

            Assert.IsTrue(Checker.Passed);
        }

        [Aspect(typeof(FilterTests_Aspect))]
        public class FilterTests_Target
        {
            [Aspect(typeof(FilterTests_Aspect), NameFilter = "Do")]
            public void Do123()
            {
            }
        }

        public class FilterTests_Aspect
        {
            public int Counter = 0;

            [Advice(Advice.Type.After, Advice.Target.Method)]
            public void AfterMethod()
            {
                Counter++;
                Checker.Passed = Counter == 1;
            }
        }
    }
}