using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspectInjector.Broker;

namespace AspectInjector.Tests.General
{
    [TestClass]
    public class AspectScopeTests
    {
        [TestMethod]
        public void SCOPE_Create_Aspect_Per_Instance()
        {
            AspectScopeTests_PerInstanceAspect._counter = 0;
            for (int i = 0; i < 10; i++)
            {
                var t = new AspectScopeTests_Target();
                Assert.AreEqual(i + 1, AspectScopeTests_PerInstanceAspect._counter);
            }
        }

        [TestMethod]
        public void SCOPE_Create_Global_Aspect()
        {
            for (int i = 0; i < 10; i++)
            {
                var t = new AspectScopeTests_Target();
                Assert.AreEqual(1, AspectScopeTests_GlobalAspect._counter);
            }
        }
    }

    [Aspect(Aspect.Scope.PerInstance)]
    internal class AspectScopeTests_PerInstanceAspect
    {
        public static int _counter;

        public AspectScopeTests_PerInstanceAspect()
        {
            _counter++;
        }

        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Do()
        {
        }
    }

    [Aspect(Aspect.Scope.Global)]
    internal class AspectScopeTests_GlobalAspect
    {
        public static int _counter;

        public AspectScopeTests_GlobalAspect()
        {
            _counter++;
        }

        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void Do()
        {
        }
    }

    [Inject(typeof(AspectScopeTests_PerInstanceAspect))]
    [Inject(typeof(AspectScopeTests_GlobalAspect))]
    internal class AspectScopeTests_Target
    {
        public void TestMethod()
        {
        }
    }
}