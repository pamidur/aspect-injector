using System;

namespace AspectInjector.Test
{ 

    internal class Program
    {

        private static void Main(string[] args)
        {
            TestInterfaceInjection();
            TestMethodInjection();

            StringComparer b = default(StringComparer);

            var a = false;

            var b1 = default(StringComparer);
            var b2 = default(int);

            a = true;

            if (a == true)
                return;

            Ololo(ref a);
        }

        private static void Ololo(ref bool dd)
        {
        }

        private static void TestInterfaceInjection()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("TestInterfaceInjection");
            Console.WriteLine();

            var testInterface = (ITestInterface)new TestInterfaceProxyClass();

            testInterface.TestMethod("");
        }

        private static void TestMethodInjection()
        {
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine("TestMethodInjection");
            //Console.WriteLine();

            //var testMethod = new TestMethodClass("");

            //testMethod.TestEvent += (s, e) => { };
            //testMethod.TestEvent -= (s, e) => { };
            //testMethod.TestMethodAAA("");
            //testMethod.TestMethodFiltered2();
            //testMethod.TestProperty = 0;
            //var a = testMethod.TestProperty;
        }
    }
}
