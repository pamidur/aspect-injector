//using AspectInjector.Broker;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace AspectInjector.Tests.AdviceParameters.Arguments
//{
//    [TestClass]
//    public class ArgumentsModificationTests
//    {
//        static object new_obj = new object();
//        static int new_val = 20;

//        [TestMethod]
//        public void AdviceArguments_Arguments_Can_Be_Modified()
//        {
//            var obj = new object();
//            object outObj;
//            var val = 1;
//            int outVal;

//            Checker.Passed = false;
//            new TargetClass().TestMethod(obj, ref obj, out outObj, val, ref val, out outVal);
//            Assert.IsTrue(Checker.Passed);
//        }

//        internal class TargetClass
//        {
//            [Aspect(typeof(AspectImplementation))]
//            public void TestMethod(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut)
//            {
//                Checker.Passed = obj == new_obj && objRef == new_obj && objOut == new_obj;

//                valueOut = 1;
//                objOut = new object();
//            }
//        }

//        internal class AspectImplementation
//        {
//            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
//            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Arguments)] object[] args)
//            {
//                args[0] = new_obj;
//                args[1] = new_obj;
//                args[2] = new_obj;

//                args[4] = new_val;
//                args[5] = new_val;
//                args[6] = new_val;
//            }
//        }
//    }
//}