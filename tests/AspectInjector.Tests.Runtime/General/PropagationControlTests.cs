using AspectInjector.Broker;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.General
{
    public class PropagationControlTests
    {
        private readonly TestClass _testTarget;

        public PropagationControlTests()
        {
            _testTarget = new TestClass();
        }

        [Fact]
        public void Propagation_Options_Correctly_Processed()
        {
            _testTarget.Method();
            _testTarget.Event += (s, e) => { };
            _testTarget.Ev2ent += (s, e) => { };
            _testTarget.Property = "";
        }

        [EventTrigger]
        [PropertyTrigger]
        [MethodTrigger]
        public class TestClass
        {
            public event EventHandler Event;
            public event EventHandler Ev2ent;
            public string Property { get; set; }
            public void Method() { }
        }

        [Injection(typeof(SuccessAspect), Propagation = PropagateTo.Events, PropagationFilter = "Event")]
        public class EventTrigger : Attribute { }

        [Injection(typeof(SuccessAspect), Propagation = PropagateTo.Properties)]
        public class PropertyTrigger : Attribute { }

        [Injection(typeof(SuccessAspect), Propagation = PropagateTo.Methods)]
        public class MethodTrigger : Attribute { }


        [Aspect(Scope.Global)]        
        public class SuccessAspect
        {
            [Advice(Kind.Before, Targets =Target.Method | Target.EventAdd | Target.EventRemove | Target.Getter | Target.Setter )]
            public void Success([Argument(Source.Name)] string name, [Argument(Source.Triggers)] Attribute[] triggers)
            {
                switch (name)
                {
                    case nameof(TestClass.Event):
                        Assert.True(triggers.Length == 1 && triggers[0] is EventTrigger);break;
                    case nameof(TestClass.Property):
                        Assert.True(triggers.Length == 1 && triggers[0] is PropertyTrigger); break;
                    case nameof(TestClass.Method):
                        Assert.True(triggers.Length == 1 && triggers[0] is MethodTrigger); break;
                    default:
                        Assert.True(false);break;
                }                
            }
        }
    }
}
