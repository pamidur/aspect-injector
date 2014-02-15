using AspectInjector.Test.Aspects;
using System;

namespace AspectInjector.Test
{
    [Aspect(typeof(TestInterfaceAspect))]
    [Aspect(typeof(NotifyPropertyChangedAspect))]
    internal class TestClass
    {
        public string Value { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }

        [Aspect(typeof(TestMethodAspect))]
        public void Print()
        {
            Console.WriteLine("TestClass.Print() called");
        }
    }
}