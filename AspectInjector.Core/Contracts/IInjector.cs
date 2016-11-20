using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IInjector : IInitializable
    {
        void Apply(Aspect aspect, Injection injection);

        bool CanApply(Injection injection);
    }
}