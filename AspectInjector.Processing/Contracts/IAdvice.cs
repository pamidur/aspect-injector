using AspectInjector.Models;
using Mono.Cecil;

namespace AspectInjector.Contracts
{
    public interface IAdvice
    {
        TypeReference HostType { get; }

        bool IsApplicableFor(Aspect aspect);
    }
}