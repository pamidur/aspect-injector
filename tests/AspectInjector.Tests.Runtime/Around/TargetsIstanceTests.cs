using AspectInjector.Tests.Assets;
using System.Collections.Generic;
using Xunit;

namespace AspectInjector.Tests.Runtime.Around
{
    public class TargetsInstanceTests : TestRunner
    {
        [Fact]
        public void AdviceAround_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestPropertySetterExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestStaticPropertySetterExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestPropertyGetterExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestStaticPropertyGetterExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestEventAddExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestStaticEventAddExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestEventRemoveExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,

                Events.TestStaticEventRemoveExit,
                InstanceAspect.AroundExit
            });
        }

        [Fact]
        public void AdviceAround_Instance_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,
                Events.FactExit,
                InstanceAspect.AroundExit,

                //state machine executes after around
                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestIteratorMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestAsyncMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestAsyncMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestAsyncMethodExit,
            });
        }

        [Fact]
        public void AdviceAround_Instance_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                InstanceAspect.AroundEnter,
                Events.TestStaticMethodExit,
                InstanceAspect.AroundExit,

                //state machine executes after around
                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestStaticIteratorMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,

                InstanceAspect.AroundEnter,
                InstanceAspect.AroundExit,
                Events.TestStaticAsyncMethodExit,
            });
        }
    }
}