using AspectInjector.Contexts;
using System.Collections.Generic;

namespace AspectInjector.Contracts
{
    public interface IReferencedAdviceProvider
    {
        void Init(ProcessingContext context);

        IEnumerable<T> ReadAdvices<T>() where T : IAdvice;
    }
}