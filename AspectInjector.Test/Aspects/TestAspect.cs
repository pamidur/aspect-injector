using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    class TestAspect : ICustomAspect
    {
        public TestAspect()
        {
            Console.WriteLine("Constructor");
        }

        public void Before()
        {
            Console.WriteLine("Before aspect");
        }

        public void After()
        {
            Console.WriteLine("After aspect");
        }
    }
}
