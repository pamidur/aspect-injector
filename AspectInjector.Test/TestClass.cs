using AspectInjector.Test.Aspects;
using System;

namespace AspectInjector.Test
{
    internal class TestClass
    {
        [Aspect(typeof(NotifyPropertyChangedAspect))]
        public string Value { get; set; }

        [Aspect(typeof(TestAspect))]
        public void Print()
        {
            Console.WriteLine("Original text");
        }

        [Aspect(typeof(NewTestMethodAspect))]
        public void Print2()
        {
            Console.WriteLine("New method aspect");
        }
    }
}