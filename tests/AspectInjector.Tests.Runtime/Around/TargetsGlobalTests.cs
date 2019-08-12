using AspectInjector.Tests.Assets;
using System.Collections.Generic;
using Xunit;

namespace AspectInjector.Tests.Runtime.Around
{
    public class Global_Tests : TestRunner
    {
        protected virtual string EnterToken => GlobalAspect.AroundEnter;
        protected virtual string ExitToken => GlobalAspect.AroundExit;

        [Fact]
        public void AdviceAround_Static_Constructor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticConstructorEnter,
                Events.TestStaticConstructorExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Constructor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestConstructorEnter,
                Events.TestConstructorExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestPropertySetterEnter,
                Events.TestPropertySetterExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticPropertySetterEnter,
                Events.TestStaticPropertySetterExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestPropertyGetterEnter,
                Events.TestPropertyGetterExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticPropertyGetterEnter,
                Events.TestStaticPropertyGetterExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestEventAddEnter,
                Events.TestEventAddExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticEventAddEnter,
                Events.TestStaticEventAddExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestEventRemoveEnter,
                Events.TestEventRemoveExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticEventRemoveEnter,
                Events.TestStaticEventRemoveExit,
                ExitToken
            });
        }

        [Fact]
        public void AdviceAround_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                EnterToken,
                Events.FactEnter,
                Events.FactExit,
                ExitToken,

                //state machine executes after around
                EnterToken,
                ExitToken,
                Events.TestIteratorMethodExit,

                EnterToken,
                ExitToken,
                Events.TestAsyncMethodExit,

                EnterToken,
                ExitToken,
                Events.TestAsyncMethodExit,

                EnterToken,
                ExitToken,
                Events.TestAsyncMethodExit,
            });
        }

        [Fact]
        public void AdviceAround_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                EnterToken,
                Events.TestStaticMethodEnter,
                Events.TestStaticMethodExit,
                ExitToken,

                //state machine executes after around
                EnterToken,
                ExitToken,
                Events.TestStaticIteratorMethodExit,

                EnterToken,
                ExitToken,
                Events.TestStaticAsyncMethodExit,

                EnterToken,
                ExitToken,
                Events.TestStaticAsyncMethodExit,

                EnterToken,
                ExitToken,
                Events.TestStaticAsyncMethodExit,
            });
        }
    }
}