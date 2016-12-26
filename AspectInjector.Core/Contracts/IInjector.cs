using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IInjector : IInitializable
    {
        byte Priority { get; }

        void Apply(Aspect aspect, Injection injection);

        bool CanApply(Injection injection);
    }
}