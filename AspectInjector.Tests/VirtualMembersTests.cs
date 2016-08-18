using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AspectInjector.Tests
{
    [TestClass]
    public class VirtualMembersTests
    {
        [TestMethod]
        public void Inject_Advice_Around_VirtualAndOverride()
        {
            VirtualMembersTests_Base t = new VirtualMembersTests_Inherited();
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


    [Aspect(typeof(VirtualMembersTests_Aspect))]
    internal class VirtualMembersTests_Base
    {
        public virtual void Test()
        {
        }
    }

    [Aspect(typeof(VirtualMembersTests_Aspect))]
    internal class VirtualMembersTests_Inherited : VirtualMembersTests_Base
    {
        public override void Test()
        {
            base.Test();
        }
    }

    internal class VirtualMembersTests_Aspect
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
