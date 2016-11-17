using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IInjector : IInitializable
    {
        void Apply(Aspect aspect, Advice advice);

        bool CanApply(Advice advice);
    }
}