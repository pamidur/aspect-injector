using AspectInjector.Broker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace AspectInjector.Tests.Advices
{
    [TestClass]
    public class ArgumentsTests
    {
        [TestMethod]
        public void AdviceArguments_Instance_InjectBeforeMethod_NotNull()
        {
            Checker.Passed = false;
            new ArgumentsTests_InstanceTarget().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Instance_InjectBeforeStaticMethod_Null()
        {
            Checker.Passed = false;
            ArgumentsTests_StaticInstanceTarget.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_ReturnType_InjectBeforeMethod()
        {
            Checker.Passed = false;
            ArgumentsTests_ReturnTypeTarget.TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectBeforeStaticMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_StaticMethodTarget().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectBeforeStaticConstructor()
        {
            Checker.Passed = false;
            var temp = new ArgumentsTests_StaticConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectBeforeMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_MethodTarget().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectBeforeConstructor()
        {
            Checker.Passed = false;
            var temp = new ArgumentsTests_ConstructorTarget();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectBeforeConstructorChain()
        {
            Checker.Passed = false;
            new ArgumentsTests_ConstructorChainTarget().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Method_InjectAroundMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_AroundMethodTarget().TestMethod();
            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Arguments_InjectBeforeMethod()
        {
            var obj = new object();
            object outObj;
            var val = 1;
            int outVal;

            Checker.Passed = false;

            new ArgumentsTests_ArgumentsTarget().TestMethod(obj,
                ref obj,
                out outObj,
                val,
                ref val,
                out outVal);

            Assert.IsTrue(Checker.Passed);
        }

        [TestMethod]
        public void AdviceArguments_Arguments_InjectBeforeStaticMethod()
        {
            Checker.Passed = false;
            ArgumentsTests_StaticArgumentsTarget.TestMethod(1, "2");
            Assert.IsTrue(Checker.Passed);
        }

        internal class ArgumentsTests_InstanceTarget
        {
            [Inject(typeof(ArgumentsTests_InstanceAspect))]
            public void TestMethod()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_InstanceAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Instance)] object instance)
            {
                Checker.Passed = instance != null;
            }
        }

        internal static class ArgumentsTests_StaticInstanceTarget
        {
            [Inject(typeof(ArgumentsTests_StaticInstanceAspect))]
            public static void TestMethod()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_StaticInstanceAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Instance)] object instance)
            {
                Checker.Passed = instance == null;
            }
        }

        [Inject(typeof(ArgumentsTests_ReturnTypeAspect))]
        internal static class ArgumentsTests_ReturnTypeTarget
        {
            public static void TestMethod()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_ReturnTypeAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Type)] System.Type type)
            {
                Checker.Passed = type == typeof(ArgumentsTests_ReturnTypeTarget);
            }
        }

        internal class ArgumentsTests_StaticMethodTarget
        {
            [Inject(typeof(ArgumentsTests_StaticMethodAspect))]
            public void TestMethod()
            {
            }
        }

        internal class ArgumentsTests_StaticConstructorTarget
        {
            [Inject(typeof(ArgumentsTests_StaticMethodAspect))]
            static ArgumentsTests_StaticConstructorTarget()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_StaticMethodAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method | Advice.Target.Constructor)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method != null;
            }
        }

        internal class ArgumentsTests_MethodTarget
        {
            [Inject(typeof(ArgumentsTests_MethodAspect))]
            public void TestMethod()
            {
            }
        }

        internal class ArgumentsTests_ConstructorChainTarget
        {
            public ArgumentsTests_ConstructorChainTarget()
            {
            }

            public ArgumentsTests_ConstructorChainTarget(int a) : this()
            {
            }

            public ArgumentsTests_ConstructorChainTarget(int a, int b) : this(a)
            {
            }

            [Inject(typeof(ArgumentsTests_MethodAspect))]
            public void TestMethod()
            {
            }
        }

        [Inject(typeof(ArgumentsTests_MethodAspect))]
        internal class ArgumentsTests_ConstructorTarget
        {
            [Inject(typeof(ArgumentsTests_MethodAspect))]
            public ArgumentsTests_ConstructorTarget()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_MethodAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method | Advice.Target.Constructor)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method != null;
            }
        }

        internal class ArgumentsTests_ArgumentsTarget
        {
            [Inject(typeof(ArgumentsTests_ArgumentsAspect))]
            public void TestMethod(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut)
            {
                valueOut = 1;
                objOut = new object();
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_ArgumentsAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Arguments)] object[] args)
            {
                Checker.Passed =
                    args[0] != null && args[1] != null && args[2] == null &&
                    (int)args[3] == 1 && (int)args[4] == 1 && (int)args[5] == 0;
            }
        }

        internal static class ArgumentsTests_StaticArgumentsTarget
        {
            [Inject(typeof(ArgumentsTests_StaticArgumentsAspect))]
            public static void TestMethod(int a, string b)
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_StaticArgumentsAspect
        {
            [Advice(Advice.Type.Before, Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Arguments)] object[] args)
            {
                Checker.Passed = (int)args[0] == 1 && (string)args[1] == "2";
            }
        }

        internal class ArgumentsTests_AroundMethodTarget
        {
            [Inject(typeof(ArgumentsTests_AroundMethodAspect))]
            public void TestMethod()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        internal class ArgumentsTests_AroundMethodAspect
        {
            [Advice(Advice.Type.Around, Advice.Target.Method)]
            public object BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method.Name == "TestMethod";
                return null;
            }
        }
    }
}