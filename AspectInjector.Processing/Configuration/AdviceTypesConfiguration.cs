using AspectInjector.Core.Contracts;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Configuration
{
    public class AdviceTypesConfiguration<T> : AdviceTypesConfiguration
        where T : IAdvice
    {
        private readonly ProcessingConfiguration _config;

        public AdviceTypesConfiguration(ProcessingConfiguration config)
        {
            _config = config;
        }

        public AdviceTypesConfiguration<T> SetExtractor<TE>()
            where TE : class, IAdviceExtractor<T>
        {
            Extractor = typeof(TE);
            return this;
        }

        public AdviceTypesConfiguration<T> AddInjector<TI>()
            where TI : class, IInjector<T>
        {
            Injectors.Add(typeof(TI));
            return this;
        }

        public ProcessingConfiguration Done()
        {
            return _config;
        }
    }

    public abstract class AdviceTypesConfiguration
    {
        public Type Type { get; set; }

        public Type Extractor { get; set; }

        public ICollection<Type> Injectors { get; set; } = new List<Type>();
    }
}