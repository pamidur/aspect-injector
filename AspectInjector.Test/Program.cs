using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = new TestClass();
            test.Print();
            test.Print2();
        }
    }
}
