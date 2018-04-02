using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before
{
    
    public class TargetsGlobalTests : TestRunner
    {
        [Fact]
        public void AdviceBefore_Global_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Global_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.FactEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestIteratorMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestAsyncMethodEnter,
            });
        }

        [Fact]
        public void AdviceBefore_Global_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                GlobalAspect.BeforeExecuted,
                Events.TestStaticMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestStaticIteratorMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,

                GlobalAspect.BeforeExecuted,
                Events.TestStaticAsyncMethodEnter,
            });
        }
    }
}