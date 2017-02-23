using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.After.Instance
{
    [TestClass]
    public class TargetsInstanceTests : TestRunner
    {
        [TestMethod]
        public void AdviceAfter_Instance_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Events.TestConstructorExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestMethodExit,
                AfterAspectInstance.Executed,

                Events.TestIteratorMethodExit,
                AfterAspectInstance.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectInstance.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectInstance.Executed,

                Events.TestAsyncMethodExit,
                AfterAspectInstance.Executed
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestStaticMethodExit,
                AfterAspectInstance.Executed,

                Events.TestStaticIteratorMethodExit,
                AfterAspectInstance.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectInstance.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectInstance.Executed,

                Events.TestStaticAsyncMethodExit,
                AfterAspectInstance.Executed
            });
        }
    }
}