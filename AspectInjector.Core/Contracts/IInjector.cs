using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IInjector : IInitializable
    {
        void Apply(Aspect aspect, IAdvice advice);

        bool CanApply(IAdvice advice);
    }
}