using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspectInjector.Broker;

namespace AspectInjector.Tests.AdviceInjectionTests
{
    [TestClass]
    public class AspectScopeTest
    {
        [TestMethod]
        public void Create_Aspect_Per_Instance()
        {
            for(int i = 0; i < 10; i++)
            {
                var t = new PerInstanceTarget();
                t.Do();
                Assert.AreEqual(1, t.Counter);
            }
        }

        [TestMethod]
        public void Create_Aspect_Per_Type()
        {
            for (int i = 0; i < 10; i++)
            {
                var t = new PerTypeTarget();
                t.Do();
                Assert.AreEqual(i + 1, t.Counter);
            }
        }
    }

    [AspectScope(AspectScope.Instance)]
    internal class PerInstanceAspect
    {
        private int _counter;

        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void IncrementCounter()
        {
            _counter++;
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Getter)]
        public int GetCounter([AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort)
        {
            abort = true;
            return _counter;
        }
    }

    [AspectScope(AspectScope.Type)]
    internal class PerTypeAspect
    {
        private int _counter;

        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
        public void IncrementCounter()
        {
            _counter++;
        }

        [Advice(InjectionPoints.Before, InjectionTargets.Getter)]
        public int GetCounter([AdviceArgument(AdviceArgumentSource.AbortFlag)] ref bool abort)
        {
            abort = true;
            return _counter;
        }
    }

    [Aspect(typeof(PerInstanceAspect))]
    internal class PerInstanceTarget
    {
        public int Counter
        {
            get { return 0; }
        }

        public void Do()
        {

        }
    }

    [Aspect(typeof(PerTypeAspect))]
    internal class PerTypeTarget
    {
        public int Counter
        {
            get { return 0; }
        }

        public void Do()
        {

        }
    }
}
