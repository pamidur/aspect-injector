using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IWeaver
    {
        byte Priority { get; }

        void Weave(Injection injection);

        bool CanWeave(Injection injection);
    }
}