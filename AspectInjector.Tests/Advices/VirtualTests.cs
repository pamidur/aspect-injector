using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class VirtualTests
    {
        [TestMethod]
        public void Advices_InjectAroundVirtualOverrideMethods()
        {
            VirtualTests_Base t = new VirtualTests_Inherited();
            try
            {
                t.Test();
            }
            catch(Exception e)
            {
                Assert.Fail("No exception is expected, but got {0}", e);
            }
        }
    }


    [Aspect(typeof(VirtualTests_Aspect))]
    internal class VirtualTests_Base
    {
        public virtual void Test()
        {
        }
    }

    [Aspect(typeof(VirtualTests_Aspect))]
    internal class VirtualTests_Inherited : VirtualTests_Base
    {
        public override void Test()
        {
            base.Test();
        }
    }

    internal class VirtualTests_Aspect
    {
        private int counter = 0;

        [Advice(InjectionPoints.Around, InjectionTargets.Method)]
        public object Trace(
            [AdviceArgument(AdviceArgumentSource.Type)] Type type,
            [AdviceArgument(AdviceArgumentSource.Name)] string methodName,
            [AdviceArgument(AdviceArgumentSource.Target)] Func<object[], object> target,
            [AdviceArgument(AdviceArgumentSource.Arguments)] object[] arguments)
        {
            if (counter > 0)
            {
                throw new Exception("Advice method was called more than one time");
            }

            counter++;

            return target(arguments);
        }
    }
}
