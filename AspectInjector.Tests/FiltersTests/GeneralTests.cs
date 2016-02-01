using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.FiltersTests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void Same_Aspect_Called_Once_On_Multiple_Filters_Match()
        {
            Checker.Passed = false;

            var a = new TestClass();
            a.Do123();

            Assert.IsTrue(Checker.Passed);
        }

        [Aspect(typeof(TestAspectImplementation))]
        public class TestClass
        {
            [Aspect(typeof(TestAspectImplementation), NameFilter = "Do")]
            public void Do123()
            {
            }
        }

        public class TestAspectImplementation
        {
            public int Counter = 0;

            [Advice(InjectionPoints.After, InjectionTargets.Method)]
            public void AfterMethod()
            {
                Counter++;
                Checker.Passed = Counter == 1;
            }
        }
    }
}