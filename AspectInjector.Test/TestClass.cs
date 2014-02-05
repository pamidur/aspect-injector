using AspectInjector.Test.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    internal class TestClass
    {
        [Aspect(Type = typeof(NotifyPropertyChangedAspect))]
        public string Value { get; set; }

        [Aspect(Type = typeof(TestAspect))]
        public void Print()
        {
            Console.WriteLine("Original text");
        }

        [Aspect(Type = typeof(NewTestMethodAspect))]
        public void Print2()
        {
            Console.WriteLine("New method aspect");
        }
    }
}
