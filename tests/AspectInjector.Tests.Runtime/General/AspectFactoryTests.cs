//using AspectInjector.Broker;
//using Xunit;
//using System;

//namespace AspectInjector.Tests.General
//{
    
//    public class AspectFactoryTests
//    {
//        [Fact]
//        public void General_AspectFactory_CreateAspect()
//        {
//            Checker.Passed = false;
//            var test = new AspectFactoryTests_Target();
//            Assert.True(Checker.Passed);
//        }
//    }

//    [Inject(typeof(AspectFactoryTests_Aspect))]
//    public class AspectFactoryTests_Target
//    {
//    }

//    [Aspect(Aspect.Scope.PerInstance, Factory = typeof(AspectFactory))]
//    public class AspectFactoryTests_Aspect
//    {
//        private static object aaa;

//        [Advice(Advice.Type.After, Advice.Target.Constructor)]
//        public void Fact()
//        {
//        }

//        private static void ololo()
//        {
//            aaa = AspectFactory.GetInstance(typeof(AspectFactoryTests_Aspect));
//        }
//    }

//    public class AspectFactory
//    {
//        public static object GetInstance(Type aspectType)
//        {
//            Checker.Passed = true;
//            return Activator.CreateInstance(aspectType);
//        }
//    }
//}