using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before
{
    [TestClass]
    public class TargetsTests : TestRunner
    {
        [TestMethod]
        public void AdvicesBefore_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                BeforeAspectGlobal.Executed,
                Events.TestConstructorEnter
            });
        }

        [TestMethod]
        public void AdvicesBefore_Methods()
        {
            ExecMethod();
            EnterIteratorMethod();
            ExecAsyncVoidMethod();
            EnterAsyncTaskMethod();
            EnterAsyncTypedTaskMethod();

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
    }
}