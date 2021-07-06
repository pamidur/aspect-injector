using System;
using System.Threading;
using System.Threading.Tasks;
using Logging;

namespace AspectInjector.Samples.Logging.Services
{
    [AnyMethodHandleIt]
    [AnyMethodHandleIt2]
    public class SampleService
    {
        public int GetCount()
        {
            Thread.Sleep(2);
            Console.WriteLine("GetCount");
            return 10;
        }

        public async Task FireTask()
        {
            Console.WriteLine("FireTask");
            await Task.Delay(2);
        }

        public async Task<string> FireTask2()
        {
            await Task.Delay(2);
            Console.WriteLine("FireTask2");
            return "asds";
        }

        public void ThrowGetCount()
        {
            Console.WriteLine("ThrowGetCount");

            throw new InvalidOperationException();
        }
    }
}