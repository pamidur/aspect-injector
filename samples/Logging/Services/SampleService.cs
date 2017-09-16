using AspectInjector.Broker;
using AspectInjector.Samples.Logging.Aspects;
using System.Threading;

namespace AspectInjector.Samples.Logging.Services
{
    [Inject(typeof(LoggingAspect))]
    public class SampleService
    {
        public int GetCount()
        {
            Thread.Sleep(3000);

            return 10;
        }
    }
}
