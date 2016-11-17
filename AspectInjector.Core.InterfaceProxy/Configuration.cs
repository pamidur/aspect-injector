using AspectInjector.Core.Configuration;
using AspectInjector.Core.InterfaceProxy;

namespace AspectInjector.Core
{
    public static class InterfaceProxyConfigurationExtensions
    {
        public static ProcessingConfiguration UseInterfaceProxyInjection(this ProcessingConfiguration config)
        {
            return config
                .RegisterAdviceExtractor<InterfaceAdviceExtractor>()
                .RegisterInjector<InterfaceInjector>();
        }
    }
}