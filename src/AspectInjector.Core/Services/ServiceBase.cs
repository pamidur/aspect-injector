namespace AspectInjector.Core.Services
{
    public abstract class ServiceBase
    {
        protected Logger Log { get; private set; }

        protected ServiceBase(Logger logger)
        {
            Log = logger;
        }
    }
}