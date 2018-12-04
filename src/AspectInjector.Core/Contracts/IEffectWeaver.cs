using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectWeaver
    {
        byte Priority { get; }

        void Weave(InjectionDefinition injection);

        bool CanWeave(InjectionDefinition injection);
    }
}