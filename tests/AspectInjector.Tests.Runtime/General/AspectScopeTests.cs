//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using AspectInjector.Broker;

//namespace AspectInjector.Tests.General
//{
//    [TestClass]
//    public class AspectScopeTests
//    {
//        [TestMethod]
//        public void Create_Aspect_Per_Instance()
//        {
//            for(int i = 0; i < 10; i++)
//            {
//                var t = new AspectScopeTests_PerInstanceTarget();
//                t.TestMethod();
//                Assert.AreEqual(1, t.Counter);
//            }
//        }

//        [TestMethod]
//        public void Create_Aspect_Per_Type()
//        {
//            for (int i = 0; i < 10; i++)
//            {
//                var t = new AspectScopeTests_PerTypeTarget();
//                t.TestMethod();
//                Assert.AreEqual(i + 1, t.Counter);
//            }
//        }
//    }

//    [AspectScope(AspectScope.Instance)]
//    internal class AspectScopeTests_PerInstanceAspect
//    {
//        private int _counter;

//        [Advice(Advice.Type.Before, Advice.Target.Method)]
//        public void IncrementCounter()
//        {
//            _counter++;
//        }

//        [Advice(Advice.Type.Before, Advice.Target.Getter)]
//        public int GetCounter([Advice.Argument(Advice.Argument.Source.AbortFlag)] ref bool abort)
//        {
//            abort = true;
//            return _counter;
//        }
//    }

//    [AspectScope(AspectScope.Type)]
//    internal class AspectScopeTests_PerTypeAspect
//    {
//        private int _counter;

//        [Advice(Advice.Type.Before, Advice.Target.Method)]
//        public void IncrementCounter()
//        {
//            _counter++;
//        }

//        [Advice(Advice.Type.Before, Advice.Target.Getter)]
//        public int GetCounter([Advice.Argument(Advice.Argument.Source.AbortFlag)] ref bool abort)
//        {
//            abort = true;
//            return _counter;
//        }
//    }

//    [Aspect(typeof(AspectScopeTests_PerInstanceAspect))]
//    internal class AspectScopeTests_PerInstanceTarget
//    {
//        public int Counter
//        {
//            get { return 0; }
//        }

//        public void TestMethod()
//        {
//        }
//    }

//    [Aspect(typeof(AspectScopeTests_PerTypeAspect))]
//    internal class AspectScopeTests_PerTypeTarget
//    {
//        public int Counter
//        {
//            get { return 0; }
//        }

//        public void TestMethod()
//        {
//        }
//    }
//}