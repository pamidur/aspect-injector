using AspectInjector.Samples.Logging.Services;

namespace AspectInjector.Samples.Logging
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new SampleService();
            service.GetCount();
        }
    }
}
