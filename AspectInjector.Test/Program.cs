using AspectInjector.Broker;
using System;

namespace AspectInjector.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Test(StringComparison.InvariantCulture);
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

        [Aspect(typeof(TestMethodFilteredAspect2))]
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

        [Aspect(typeof(TestMethodFilteredAspect2))]
        [Aspect(typeof(TestMethodFilteredAspect))]
        public int Do4(bool a)
        {
            throw new NotImplementedException();
        }

        [Aspect(typeof(TestMethodFilteredAspect3))]
        public int Do5()
        {
            return 1;
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