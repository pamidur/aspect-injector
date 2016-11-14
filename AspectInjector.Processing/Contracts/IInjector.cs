using AspectInjector.Contexts;
using AspectInjector.Models;

namespace AspectInjector.Contracts
{
    public interface IInjector<T>
        where T : IAdvice
    {
        void Init(ProcessingContext context);

        void Apply(Aspect aspect, T advice);
    }
}