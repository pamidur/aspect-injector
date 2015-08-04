using Mono.Cecil;

namespace AspectInjector.BuildTask.Contexts
{
    internal class TargetAsyncMethodContext : TargetMethodContext
    {
        public TargetAsyncMethodContext(MethodDefinition targetMethod) : base(targetMethod)
        {
        }
    }
}