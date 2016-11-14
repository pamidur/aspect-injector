using AspectInjector.Contracts;
using System;
using System.Collections.Generic;

namespace AspectInjector.Configuration
{
    public class AdviceTypeConfig<T> : AdviceTypeConfig
        where T : IAdvice
    {
        public AdviceTypeConfig<T> SetExtractor<TE>()
            where TE : IAdviceExtractor<T>
        {
            Extractor = typeof(TE);
            return this;
        }

        public AdviceTypeConfig<T> AddInjector<TI>()
            where TI : IInjector<T>
        {
            Injectors.Add(typeof(TI));
            return this;
        }
    }

    public abstract class AdviceTypeConfig
    {
        public Type AdviceType { get; set; }

        public Type Extractor { get; set; }

        public ICollection<Type> Injectors { get; set; } = new List<Type>();
    }
}