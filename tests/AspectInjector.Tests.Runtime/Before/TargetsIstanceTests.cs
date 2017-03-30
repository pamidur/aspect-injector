using AspectInjector.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before
{
    [TestClass]
    public class TargetsInstanceTests : TestRunner
    {
        [TestMethod]
        public void AdviceBefore_Instance_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestEventRemoveEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticEventRemoveEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestIteratorMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestStaticIteratorMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,

                InstanceAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,
            });
        }
    }
}