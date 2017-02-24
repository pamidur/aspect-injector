using AspectInjector.Tests.Assets;
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
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                GlobalAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                GlobalAspect.AfterExecuted
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
                GlobalAspect.AfterExecuted,

                Events.TestIteratorMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                GlobalAspect.AfterExecuted
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
                GlobalAspect.AfterExecuted,

                Events.TestStaticIteratorMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                GlobalAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                GlobalAspect.AfterExecuted
            });
        }
    }
}