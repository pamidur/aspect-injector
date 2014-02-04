using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    class TestClass
    {
        [CustomAspect(Type = typeof(TestAspect))]
        public void Print()
        {
            Console.WriteLine("Original text");
        }
    }
}
