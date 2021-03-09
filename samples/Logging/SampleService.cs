using System;
using System.Threading;
using Logging;

namespace AspectInjector.Samples.Logging.Services
{
    [AnyMethodHandleIt]
    public class SampleService
    {
        public int GetCount()
        {
            Thread.Sleep(1000);

            return 10;
        }

        public void ThrowGetCount()
        {
            throw new ArgumentNullException();
        }
    }
}