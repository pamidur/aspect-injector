using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IEffectWeaver
    {
        byte Priority { get; }

        void Weave(Injection injection);
    }
}