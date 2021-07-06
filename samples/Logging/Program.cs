using AspectInjector.Samples.Logging.Services;
using System.Threading.Tasks;

namespace AspectInjector.Samples.Logging
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new SampleService();
            service.GetCount();
            try
            {
                service.ThrowGetCount();
            }
            catch { }
            await service.FireTask();
            await service.FireTask2();
        }
    }
}
