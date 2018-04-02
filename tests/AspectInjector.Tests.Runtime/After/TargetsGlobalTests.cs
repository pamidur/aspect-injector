using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.After
{
    public class TargetsGlobalTests : TestRunner
    {
        [Fact]
        public void AdviceAfter_Global_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Events.TestConstructorExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                GlobalAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Global_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.FactExit,
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

        [Fact]
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