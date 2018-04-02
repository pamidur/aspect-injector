using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.Before
{
    
    public class TargetsInstanceTests : TestRunner
    {
        [Fact]
        public void AdviceBefore_Instance_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticConstructorEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticPropertySetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticPropertyGetterEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticEventAddEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.TestStaticEventRemoveEnter
            });
        }

        [Fact]
        public void AdviceBefore_Instance_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                InstanceAspect.BeforeExecuted,
                Events.FactEnter,

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

        [Fact]
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