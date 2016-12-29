using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IWeaver : IInitializable
    {
        byte Priority { get; }

        void Apply(Injection aspect, Effect injection);

        bool CanApply(Effect injection);
    }
}