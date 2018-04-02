using AspectInjector.Tests.Assets;
using System.Collections.Generic;
using Xunit;

namespace AspectInjector.Tests.Runtime.Around
{
    public class TargetsGlobalTests : TestRunner
    {
        [Fact]
        public void AdviceAround_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestPropertySetterExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestStaticPropertySetterExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestPropertyGetterExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestStaticPropertyGetterExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestEventAddExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestStaticEventAddExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestEventRemoveExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,

                Events.TestStaticEventRemoveExit,
                GlobalAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Global_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,
                Events.FactExit,
                GlobalAspect.AroundExit,

                //state machine executes after around
                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestIteratorMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestAsyncMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestAsyncMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestAsyncMethodExit,
            });
        }

        [Fact]
        public void AdviceAround_Global_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                GlobalAspect.AroundEnter,
                Events.TestStaticMethodExit,
                GlobalAspect.AroundExit,

                //state machine executes after around
                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestStaticIteratorMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,

                GlobalAspect.AroundEnter,
                GlobalAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,
            });
        }
    }
}