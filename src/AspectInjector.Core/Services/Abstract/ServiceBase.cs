using AspectInjector.Core.Contracts;

namespace AspectInjector.Core.Services.Abstract
{
    public abstract class ServiceBase
    {
        protected ServiceBase(string prefix, ILogger logger)
        {
        }
    }
}