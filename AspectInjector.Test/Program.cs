using System;

namespace AspectInjector.Test
{
    internal class A
    {
        public static A Create()
        {
            return new A();
        }
    }

    internal class Program
    {
        private readonly A inst = A.Create();
        private readonly A inst2 = new A();

        private static void Main(string[] args)
        {
            TestInterfaceInjection();
            TestMethodInjection();

            StringComparer b = default(StringComparer);

            var a = false;

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

            testInterface.TestEvent += (s, e) => { };
            testInterface.TestEvent -= (s, e) => { };
            testInterface.TestMethod("");
            testInterface.TestProperty = 0;
            var a = testInterface.TestProperty;
        }

        private static void TestMethodInjection()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("TestMethodInjection");
            Console.WriteLine();

            var testMethod = new TestMethodClass("");

            testMethod.TestEvent += (s, e) => { };
            testMethod.TestEvent -= (s, e) => { };
            testMethod.TestMethodAAA("");
            testMethod.TestMethodFiltered2();
            testMethod.TestProperty = 0;
            var a = testMethod.TestProperty;
        }
    }
}