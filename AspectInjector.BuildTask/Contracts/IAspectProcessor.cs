using AspectInjector.BuildTask.Contexts;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Contracts
{
    public interface IAspectProcessor
    {
        bool CanProcess(TypeDefinition aspectType);
        void Process(AspectContext context);
    }
}