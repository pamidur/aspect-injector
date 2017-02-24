using AspectInjector.Tests.Assets;
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
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                InstanceAspect.AfterExecuted
            });
        }

        [TestMethod]
        public void AdviceAfter_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                InstanceAspect.AfterExecuted
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
                InstanceAspect.AfterExecuted,

                Events.TestIteratorMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted
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
                InstanceAspect.AfterExecuted,

                Events.TestStaticIteratorMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted
            });
        }
    }
}