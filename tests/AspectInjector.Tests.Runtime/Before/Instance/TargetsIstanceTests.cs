using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before.Instance
{
    [TestClass]
    public class TargetsInstanceTests : TestRunner
    {
        [TestMethod]
        public void AdviceBefore_Instance_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestStaticConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestStaticPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestStaticEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
                Events.TestEventRemoveEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                BeforeAspectInstance.Executed,
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
                BeforeAspectInstance.Executed,
                Events.TestMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestIteratorMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestAsyncMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestAsyncMethodEnter,

                BeforeAspectInstance.Executed,
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
                BeforeAspectInstance.Executed,
                Events.TestStaticMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestStaticIteratorMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestStaticAsyncMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestStaticAsyncMethodEnter,

                BeforeAspectInstance.Executed,
                Events.TestStaticAsyncMethodEnter,
            });
        }
    }
}