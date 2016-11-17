using AspectInjector.Core.Defaults;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.InterfaceProxy
{
    internal class InterfaceInjector : DefaultInjectorBase<InterfaceAdvice>
    {
        protected override void Apply(Aspect<AssemblyDefinition> aspect, InterfaceAdvice advice)
        {
            base.Apply(aspect, advice);
        }
    }
}