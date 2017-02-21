using AspectInjector.Broker;
using AspectInjector.Tests.Runtime.Before;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime.Before
{
    [TestClass]
    public class TargetsTests : TestRunner
    {
        [TestMethod]
        public void AdvicesBefore_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestConstructorEnter
            });
        }
    }
}