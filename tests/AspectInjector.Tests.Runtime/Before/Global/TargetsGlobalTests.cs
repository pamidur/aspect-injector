using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before.Global
{
    [TestClass]
    public class TargetsGlobalTests : TestRunner
    {
        [TestMethod]
        public void AdviceBefore_Global_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticConstructorEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticPropertySetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticEventAddEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestEventRemoveEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticEventRemoveEnter
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestIteratorMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestAsyncMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestAsyncMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestAsyncMethodEnter,
            });
        }

        [TestMethod]
        public void AdviceBefore_Global_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestStaticMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestStaticIteratorMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestStaticAsyncMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestStaticAsyncMethodEnter,

                BeforeAspectGlobal.Executed,
                Events.TestStaticAsyncMethodEnter,
            });
        }
    }
}