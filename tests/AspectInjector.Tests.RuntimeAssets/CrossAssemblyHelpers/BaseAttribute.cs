using AspectInjector.Broker;
using System;

namespace AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers
{
    [Injection(typeof(TestAspect),Inherited = true)]
    public class BaseAttribute : Attribute
    {
    }
}
