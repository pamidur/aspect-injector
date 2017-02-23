using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.After.Global
{
    [TestClass]
    public class TargetsGlobalTests : TestRunner
    {
        [TestMethod]
        public void AdviceAfter_Global_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Events.TestConstructorExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestIteratorMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectGlobal.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestStaticMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestStaticIteratorMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectGlobal.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectGlobal.Executed
            });
        }
    }
}