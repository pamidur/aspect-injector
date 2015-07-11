using AspectInjector.Broker;
using System;

namespace AspectInjector.Test
{
    public class TestAttr : Attribute
    {
        private object _a;
        private static object _b;

        public TestAttr()
        {
            B();
        }

        public T Olololo<T>(T a)
        {
            return a;
        }

        static TestAttr()
        {
            A();
        }

        private static void A()
        {
        }

        private void B()
        {
            if (_a == null)
                _a = new object();

            if (_b == null)
                _b = new object();
        }
    }

    internal struct A
    {
        public A(int b)
            : this()
        {
            ABC = b;
        }

        public int ABC;
    }

    internal class Program
    {
        public event EventHandler<EventArgs> a;

        public StringComparer b { get; set; }

        private struct B : ITestInterface
        {
            public string TestMethod(string data)
            {
                throw new NotImplementedException();
            }

            public event EventHandler<EventArgs> TestEvent;

            public int TestProperty
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static void Main(string[] args)
        {
            var fff = (ITestInterface)new B();

            Test(StringComparison.InvariantCulture);

            var a = typeof(Int32);

            var b = (short)1;
            var b1 = (ushort)1;
            var b2 = (byte)1;
            var b3 = (sbyte)1;
            var b4 = (long)99999999999999;
            var b5 = (ulong)99999999999999;
            var b6 = (int)1;
            var b7 = (uint)1;
            var b8 = 'c';
            var b9 = (float)1.5;
            var b10 = (double)1.5;
            var b11 = true;

            var b12 = new DateTime();
            var b122 = new object[] { StringSplitOptions.None };
        }

        private static void Test(StringComparison b)
        {
        }
    }

    internal class Base { }

    [Aspect(typeof(ConstructorBeforeAspect))]
    internal class T1 : Base
    {
        //private StringBuilder a = new StringBuilder();

        [Aspect(typeof(TestMethodFilteredAspect))]
        public int Do(bool a)
        {
            if (a)
                return 1;
            else
                throw new NotImplementedException();
        }

        //[Aspect(typeof(TestMethodFilteredAspect2))]
        public int Do2(bool a)
        {
            if (a)
                return 1;
            else
                throw new NotImplementedException();
        }

        //[Aspect(typeof(TestMethodFilteredAspect2))]
        //[Aspect(typeof(TestMethodFilteredAspect))]
        public int Do3(bool a)
        {
            if (a)
                return 1;
            else
                throw new NotImplementedException();
        }

        //[Aspect(typeof(TestMethodFilteredAspect2))]
        [Aspect(typeof(TestMethodFilteredAspect), RoutableData = new object[] { StringSplitOptions.None, (short)12, true, new object[] { new string[] { "fgf" } }, new int[] { } })]
        public int Do4(bool a)
        {
            return 2;
        }

        [Aspect(typeof(TestMethodFilteredAspect))]
        [Aspect(typeof(TestMethodFilteredAspect2))]
        public int Do5(bool a)
        {
            return 3;
        }

        public int Do6()
        {
            try
            {
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public int Test1()
        {
            try
            {
                return 1;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    internal class T2 : T1
    {
        public T2()
        {
            try
            {
                int a = 1;
            }
            catch { }
        }
    }
}