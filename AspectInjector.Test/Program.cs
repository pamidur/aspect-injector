using System;

namespace AspectInjector.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = new TestClass();
            test.Print();

            test.Value = "New property value";
        }
    }
}