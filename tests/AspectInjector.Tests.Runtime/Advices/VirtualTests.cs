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
            catch (Exception e)
            {
                Assert.Fail("No exception is expected, but got {0}", e);
            }
        }
    }

    [Inject(typeof(VirtualTests_Aspect))]
    internal class VirtualTests_Base
    {
        public virtual void Test()
        {
        }
    }

    [Inject(typeof(VirtualTests_Aspect))]
    internal class VirtualTests_Inherited : VirtualTests_Base
    {
        public override void Test()
        {
            base.Test();
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class VirtualTests_Aspect
    {
        private int counter = 0;

        [Advice(Advice.Type.Around, Advice.Target.Method)]
        public object Trace(
            [Advice.Argument(Advice.Argument.Source.Type)] Type type,
            [Advice.Argument(Advice.Argument.Source.Name)] string methodName,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
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