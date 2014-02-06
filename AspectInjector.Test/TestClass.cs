using AspectInjector.Test.Aspects;
using System;

namespace AspectInjector.Test
{
    [Aspect(typeof(TestInjectInterfaceAspect))]
    internal class TestClass
    {
        private TestInjectInterfaceAspect b;
        [Aspect(typeof(NotifyPropertyChangedAspect))]
        public string Value { get; set; }

        public void Do()
        {
            ((ITestInterface)b).TestMethod("dfg");
        }

        public void Nothing()
        {
        }

        [Aspect(typeof(TestMethodAspect))]
        public void Print()
        {
            Console.WriteLine("Original text");
        }
    }
}