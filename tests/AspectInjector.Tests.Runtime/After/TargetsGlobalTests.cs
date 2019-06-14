using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.After
{
    public class Global_Tests : TestRunner
    {
        protected virtual string Token => GlobalAspect.AfterExecuted;

        [Fact]
        public void AdviceAfter_Constructor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Events.TestConstructorExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Constructor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.FactExit,
                Token,

                Events.TestIteratorMethodExit,
                Token,

                Events.TestAsyncMethodExit,
                Token,

                Events.TestAsyncMethodExit,
                Token,

                Events.TestAsyncMethodExit,
                Token
            });
        }

        [Fact]
        public void AdviceAfter_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestStaticMethodExit,
                Token,

                Events.TestStaticIteratorMethodExit,
                Token,

                Events.TestStaticAsyncMethodExit,
                Token,

                Events.TestStaticAsyncMethodExit,
                Token,

                Events.TestStaticAsyncMethodExit,
                Token
            });
        }
    }
}