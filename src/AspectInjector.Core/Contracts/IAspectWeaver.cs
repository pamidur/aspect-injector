using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface IAspectWeaver
    {
        void WeaveGlobalAssests(AspectDefinition target);
    }
}