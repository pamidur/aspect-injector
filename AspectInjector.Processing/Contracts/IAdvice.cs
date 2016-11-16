using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Contracts
{
    public interface IAdvice
    {
        TypeReference HostType { get; }

        bool IsApplicableFor(Aspect aspect);
    }
}