using AspectInjector.Broker;
using Xunit;
using System.Reflection;
using System;

namespace AspectInjector.Tests.Advices
{
    public class ArgumentsTests
    {
        [Fact]
        public void AdviceArguments_Name_Of_Real_Target()
        {
            Checker.Passed = false;
            var r = new ArgumentsTests_PropertyTarget().Fact;
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Instance_InjectBeforeMethod_NotNull()
        {
            Checker.Passed = false;
            new ArgumentsTests_InstanceTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Instance_InjectBeforeStaticMethod_Null()
        {
            Checker.Passed = false;
            ArgumentsTests_StaticInstanceTarget.Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_ReturnType_InjectBeforeMethod()
        {
            Checker.Passed = false;
            ArgumentsTests_ReturnTypeTarget.Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectBeforeStaticMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_StaticMethodTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectBeforeStaticConstructor()
        {
            Checker.Passed = false;
            var temp = new ArgumentsTests_StaticConstructorTarget();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_ReturnValue_AfterMethod()
        {
            Checker.Passed = false;
            var temp = new ArgumentsTests_AroundRetValTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectBeforeMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_MethodTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectBeforeConstructor()
        {
            Checker.Passed = false;
            var temp = new ArgumentsTests_ConstructorTarget();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectBeforeConstructorChain()
        {
            Checker.Passed = false;
            new ArgumentsTests_ConstructorChainTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Method_InjectAroundMethod()
        {
            Checker.Passed = false;
            new ArgumentsTests_AroundMethodTarget().Fact();
            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Arguments_InjectBeforeMethod()
        {
            var obj = new object();
            object outObj;
            var val = 1;
            int outVal;

            Checker.Passed = false;

            new ArgumentsTests_ArgumentsTarget().Fact(obj,
                ref obj,
                out outObj,
                val,
                ref val,
                out outVal);

            Assert.True(Checker.Passed);
        }

        [Fact]
        public void AdviceArguments_Arguments_InjectBeforeStaticMethod()
        {
            Checker.Passed = false;
            ArgumentsTests_StaticArgumentsTarget.Fact(1, "2");
            Assert.True(Checker.Passed);
        }

        internal class ArgumentsTests_InstanceTarget
        {
            [ArgumentsTests_InstanceAspect]
            public void Fact()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_InstanceAspect))]
        internal class ArgumentsTests_InstanceAspect : Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Instance)] object instance)
            {
                Checker.Passed = instance != null;
            }
        }

        internal static class ArgumentsTests_StaticInstanceTarget
        {
            [ArgumentsTests_StaticInstanceAspect]
            public static void Fact()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_StaticInstanceAspect))]
        internal class ArgumentsTests_StaticInstanceAspect:Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Instance)] object instance)
            {
                Checker.Passed = instance == null;
            }
        }

        [ArgumentsTests_ReturnTypeAspect]
        internal static class ArgumentsTests_ReturnTypeTarget
        {
            public static void Fact()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_ReturnTypeAspect))]
        internal class ArgumentsTests_ReturnTypeAspect: Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Type)] System.Type type)
            {
                Checker.Passed = type == typeof(ArgumentsTests_ReturnTypeTarget);
            }
        }

        internal class ArgumentsTests_StaticMethodTarget
        {
            [ArgumentsTests_StaticMethodAspect]
            public void Fact()
            {
            }
        }

        internal class ArgumentsTests_StaticConstructorTarget
        {
            [ArgumentsTests_StaticMethodAspect]
            static ArgumentsTests_StaticConstructorTarget()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_StaticMethodAspect))]
        internal class ArgumentsTests_StaticMethodAspect : Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method | Advice.Target.Constructor)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method != null;
            }
        }

        internal class ArgumentsTests_MethodTarget
        {
            [ArgumentsTests_MethodAspect]
            public void Fact()
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

            [ArgumentsTests_MethodAspect]
            public void Fact()
            {
            }
        }

        [ArgumentsTests_MethodAspect]
        internal class ArgumentsTests_ConstructorTarget
        {
            [ArgumentsTests_MethodAspect]
            public ArgumentsTests_ConstructorTarget()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_MethodAspect))]
        internal class ArgumentsTests_MethodAspect:Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method | Advice.Target.Constructor)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method != null;
            }
        }

        internal class ArgumentsTests_ArgumentsTarget
        {
            [ArgumentsTests_ArgumentsAspect]
            public void Fact(object obj, ref object objRef, out object objOut, int value, ref int valueRef, out int valueOut)
            {
                valueOut = 1;
                objOut = new object();
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_ArgumentsAspect))]
        internal class ArgumentsTests_ArgumentsAspect:Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Arguments)] object[] args)
            {
                Checker.Passed =
                    args[0] != null && args[1] != null && args[2] == null &&
                    (int)args[3] == 1 && (int)args[4] == 1 && (int)args[5] == 0;
            }
        }

        internal static class ArgumentsTests_StaticArgumentsTarget
        {
            [ArgumentsTests_StaticArgumentsAspect]
            public static void Fact(int a, string b)
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_StaticArgumentsAspect))]
        internal class ArgumentsTests_StaticArgumentsAspect :Attribute
        {
            [Advice(Advice.Kind.Before, Targets = Advice.Target.Method)]
            public void BeforeMethod([Advice.Argument(Advice.Argument.Source.Arguments)] object[] args)
            {
                Checker.Passed = (int)args[0] == 1 && (string)args[1] == "2";
            }
        }

        internal class ArgumentsTests_AroundMethodTarget
        {
            [ArgumentsTests_AroundMethodAspect]
            public void Fact()
            {
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_AroundMethodAspect))]
        internal class ArgumentsTests_AroundMethodAspect : Attribute
        {
            [Advice(Advice.Kind.Around, Targets = Advice.Target.Method)]
            public object BeforeMethod([Advice.Argument(Advice.Argument.Source.Method)] MethodBase method)
            {
                Checker.Passed = method.Name == "Fact";
                return null;
            }
        }

        internal class ArgumentsTests_AroundRetValTarget
        {
            [ArgumentsTests_AroundRetValAspect]
            public object Fact()
            {
                return new object();
            }
        }

        [Aspect(Aspect.Scope.Global)]
        [Injection(typeof(ArgumentsTests_AroundRetValAspect))]
        internal class ArgumentsTests_AroundRetValAspect:Attribute
        {
            [Advice(Advice.Kind.After, Targets = Advice.Target.Method)]
            public void AfterMethod([Advice.Argument(Advice.Argument.Source.ReturnValue)] object ret)
            {
                Checker.Passed = ret != null;
            }
        }

        internal class ArgumentsTests_PropertyTarget
        {
            [ArgumentsTests_PropertyTarget_Aspect]
            public object Fact { get; set; }
        }

        [Injection(typeof(ArgumentsTests_PropertyTarget_Aspect))]
        [Aspect(Aspect.Scope.Global)]
        class ArgumentsTests_PropertyTarget_Aspect : Attribute
        {
            [Advice(Advice.Kind.Before)]
            public void TestName([Advice.Argument(Advice.Argument.Source.Name)] string name)
            {
                Checker.Passed = name == "Fact";
            }
        }
    }
}