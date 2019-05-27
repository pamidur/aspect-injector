using System;
using System.Text;
using System.Threading.Tasks;

namespace UniversalWrapper
{
    class Program
    {
        private static void Main(string[] args)
        {
            var prog = new Program();

            Console.WriteLine("Run with exceptions.");
            prog.Do1(true).Wait();
            var d2 = prog.Do2(true).Result;
            var d3 = prog.Do3(true).Result;
            //prog.Do4(true); // note: you cannot really handle exceptions in `async void` methods, unless you create (or inherit) async syncronization context
            var d5 = prog.Do5(true);
            var d6 = prog.Do6(true);
            prog.Do7(true);

            Console.WriteLine("Run without exceptions.");
            prog.Do1(false).Wait();
            var d21 = prog.Do2(false).Result;
            var d31 = prog.Do3(false).Result;
            prog.Do4(false);
            var d51 = prog.Do5(false);
            var d61 = prog.Do6(false);
            prog.Do7(false);
        }

        [UniversalWrapper]
        public async Task Do1(bool error)
        {
            if (error) throw new Exception();
        }

        [UniversalWrapper]
        public async Task<int> Do2(bool error)
        {
            if (error) throw new Exception();
            return 2;
        }

        [UniversalWrapper]
        public async Task<StringBuilder> Do3(bool error)
        {
            if (error) throw new Exception();
            return new StringBuilder();
        }

        [UniversalWrapper]
        public async void Do4(bool error)
        {
            if (error)
                throw new Exception();
        }

        [UniversalWrapper]
        public object Do5(bool error)
        {
            if (error) throw new Exception();
            return new StringBuilder();
        }

        [UniversalWrapper]
        public int Do6(bool error)
        {
            if (error) throw new Exception();
            return 1;
        }

        [UniversalWrapper]
        public void Do7(bool error)
        {
            if (error) throw new Exception();
        }
    }
}
