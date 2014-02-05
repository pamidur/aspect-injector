using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    [CustomAspect(Type = typeof(TestAspect))]
    class TestClass
    {
        public void Print()
        {
            Console.WriteLine("Original text");
        }

        public void Warning()
        {
            Console.WriteLine("Original warning");
        }
    }
}
