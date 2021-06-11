using System;
using Xunit;
using AspectInjector.Broker;

namespace AspectInjector.Tests.General
{
    
    public class AspectScopeTests
    {
        [Fact]
        public void SCOPE_Create_Aspect_Per_Instance()
        {
            AspectScopeTests_PerInstanceAspect._counter = 0;
            for (int i = 0; i < 10; i++)
            {
                var t = new AspectScopeTests_Target();
                Assert.Equal(i + 1, AspectScopeTests_PerInstanceAspect._counter);
            }
        }

        [Fact]
        public void SCOPE_Create_Global_Aspect()
        {
            for (int i = 0; i < 10; i++)
            {
                var t = new AspectScopeTests_Target();
                Assert.Equal(1, AspectScopeTests_GlobalAspect._counter);
            }
        }
    }

    [Aspect(Scope.PerInstance)]
    [Injection(typeof(AspectScopeTests_PerInstanceAspect))]
    public class AspectScopeTests_PerInstanceAspect: Attribute
    {
        public static int _counter;

        public AspectScopeTests_PerInstanceAspect()
        {
            _counter++;
        }

        [Advice(Kind.Before, Targets = Target.Method)]
        public void Do()
        {
        }
    }

    [Aspect(Scope.Global)]
    [Injection(typeof(AspectScopeTests_GlobalAspect))]
    public class AspectScopeTests_GlobalAspect: Attribute
    {
        public static int _counter;

        public AspectScopeTests_GlobalAspect()
        {
            _counter++;
        }

        [Advice(Kind.Before, Targets = Target.Method)]
        public void Do()
        {
        }
    }

    [AspectScopeTests_PerInstanceAspect]
    [AspectScopeTests_GlobalAspect]
    internal class AspectScopeTests_Target
    {
        public void Fact()
        {
        }
    }
}