using AspectInjector.Broker;
using AspectInjector.Tests.Assets;
using AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers;
using System;
using Xunit;

namespace AspectInjector.Tests.Runtime.Issues
{
    public class Issue_0097
    {
        [Fact]
        public void Fixed()
        {
            new Target().Value = 12;
        }

        private class SuperTargetBase<T> : TestBaseClass<T>
        {
            [TestAspect]
            public int Number { get; set; }
        }

        private class TargetBase<T> : SuperTargetBase<T>
        {
            [TestAspect]
            public string Text { get; set; }
        }

        private class Target : TargetBase<Asset1>
        {
            [TestAspect]
            public double Value { get; set; }

            void Control()
            {
                var a = base.Text;
                var b = base.Number;
            }
        }        
    }
}
