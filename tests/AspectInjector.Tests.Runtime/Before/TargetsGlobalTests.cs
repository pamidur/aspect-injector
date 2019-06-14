using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before
{    
    public class Global_Tests : TestRunner
    {
        protected virtual string Token => GlobalAspect.BeforeExecuted;

        [Fact]
        public void AdviceBefore_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Token,
                Events.TestConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Token,
                Events.TestStaticConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Token,
                Events.TestPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Token,
                Events.TestStaticPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Token,
                Events.TestPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Token,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Token,
                Events.TestEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Token,
                Events.TestStaticEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Token,
                Events.TestEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Token,
                Events.TestStaticEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Token,
                Events.FactEnter,

                Token,
                Events.TestIteratorMethodEnter,

                Token,
                Events.TestAsyncMethodEnter,

                Token,
                Events.TestAsyncMethodEnter,

                Token,
                Events.TestAsyncMethodEnter,
            });
        }

        [Fact]
        public void AdviceBefore_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Token,
                Events.TestStaticMethodEnter,

                Token,
                Events.TestStaticIteratorMethodEnter,

                Token,
                Events.TestStaticAsyncMethodEnter,

                Token,
                Events.TestStaticAsyncMethodEnter,

                Token,
                Events.TestStaticAsyncMethodEnter,
            });
        }
    }
}