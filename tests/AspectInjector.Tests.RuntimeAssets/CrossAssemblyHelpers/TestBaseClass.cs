using System;
using System.Collections.Generic;
using System.Text;

namespace AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers
{
    [TestAspect]
    public class TestBaseClass<T>
    {        
        public SomeType SomeField { get; set; }
        public T GenericField { get; set; }
    }
}
