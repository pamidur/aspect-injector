using AspectInjector.Test.Aspects;
using System;

namespace AspectInjector.Test
{
    internal class TestClass
    {
        [Aspect(typeof(NotifyPropertyChangedAspect))]
        public string Value { get; set; }

        [Aspect(typeof(TestMethodAspect))]
        public void Print()
        {
            Console.WriteLine("Original text");
        }
    }
}