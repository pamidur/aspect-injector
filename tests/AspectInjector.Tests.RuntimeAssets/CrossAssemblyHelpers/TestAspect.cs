using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers
{
    [Aspect(Scope.PerInstance)]
    [Injection(typeof(TestAspect))]
    public class TestAspect : Attribute
    {
        [Advice(Kind.Before)]
        public void Before()
        {
            Console.WriteLine("Before");
        }
    }
}
