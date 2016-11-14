using AspectInjector.Advices.Interface;
using AspectInjector.Advices.MethodCall;
using AspectInjector.Contracts;
using AspectInjector.Defaults;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Configuration
{
    public class ProcessorConfiguration
    {
        public ILogger Log { get; set; }

        public ICollection<AdviceTypeConfig> AdviceTypes { get; private set; } = new List<AdviceTypeConfig>();

        public AdviceTypeConfig<T> RegisterAdviceType<T>()
            where T : IAdvice
        {
            var advice = new AdviceTypeConfig<T>();
            AdviceTypes.Add(advice);
            return advice;
        }

        public bool Validate()
        {
            return Log != null
                && AdviceTypes.All(a => a.Extractor != null && a.Injectors.Any());
        }

        public static ProcessorConfiguration Default { get; private set; }

        static ProcessorConfiguration()
        {
            Default = new ProcessorConfiguration
            {
                Log = new DefaultLogger()
            };

            Default.RegisterAdviceType<MethodCallAdvice>()
                .SetExtractor<MethodCallAdviceExtractor>()
                .AddInjector<MethodCallInjector>();

            Default.RegisterAdviceType<InterfaceAdvice>()
                .SetExtractor<InterfaceAdviceExtractor>()
                .AddInjector<InterfaceInjector>();
        }
    }
}