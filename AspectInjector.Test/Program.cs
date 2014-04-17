using AspectInjector.Broker;
using System;

namespace AspectInjector.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

        private static void Test(StringComparison ip)
        {
        }
    }

    internal class T1
    {
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
        [Aspect(typeof(TestMethodFilteredAspect), CustomData = new object[] { StringSplitOptions.None, (short)12, true, new object[] { new string[] { "fgf" } }, new int[] { } })]
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