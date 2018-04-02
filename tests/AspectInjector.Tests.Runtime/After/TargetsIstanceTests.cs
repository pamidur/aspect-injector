using AspectInjector.Tests.Assets;
using Xunit;
using System.Collections.Generic;

namespace AspectInjector.Tests.Runtime.After
{
    
    public class TargetsInstanceTests : TestRunner
    {
        [Fact]
        public void AdviceAfter_Instance_Consrtuctor()
        {
            ExecConstructor();
            CheckSequence(new List<string> {
                Events.TestConstructorExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Consrtuctor()
        {
            ExecStaticConstructor();
            CheckSequence(new List<string> {
                Events.TestStaticConstructorExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Setter()
        {
            ExecSetter();
            CheckSequence(new List<string> {
                Events.TestPropertySetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Setter()
        {
            ExecStaticSetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertySetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Getter()
        {
            ExecGetter();
            CheckSequence(new List<string> {
                Events.TestPropertyGetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Getter()
        {
            ExecStaticGetter();
            CheckSequence(new List<string> {
                Events.TestStaticPropertyGetterExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Add()
        {
            ExecAdd();
            CheckSequence(new List<string> {
                Events.TestEventAddExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Add()
        {
            ExecStaticAdd();
            CheckSequence(new List<string> {
                Events.TestStaticEventAddExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Remove()
        {
            ExecRemove();
            CheckSequence(new List<string> {
                Events.TestEventRemoveExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Remove()
        {
            ExecStaticRemove();
            CheckSequence(new List<string> {
                Events.TestStaticEventRemoveExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Methods()
        {
            ExecMethod();
            ExecIteratorMethod();
            ExecAsyncVoidMethod();
            ExecAsyncTaskMethod();
            ExecAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.FactExit,
                InstanceAspect.AfterExecuted,

                Events.TestIteratorMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestAsyncMethodExit,
                InstanceAspect.AfterExecuted
            });
        }

        [Fact]
        public void AdviceAfter_Instance_Static_Methods()
        {
            ExecStaticMethod();
            ExecStaticIteratorMethod();
            ExecStaticAsyncVoidMethod();
            ExecStaticAsyncTaskMethod();
            ExecStaticAsyncTypedTaskMethod();

            CheckSequence(new List<string> {
                Events.TestStaticMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticIteratorMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted,

                Events.TestStaticAsyncMethodExit,
                InstanceAspect.AfterExecuted
            });
        }
    }
}