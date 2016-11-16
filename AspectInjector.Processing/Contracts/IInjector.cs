using AspectInjector.Core.Contexts;
using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IInjector<in IAdvice> : IInitializable
    {
        void Apply(Aspect aspect, IAdvice advice);
    }
}