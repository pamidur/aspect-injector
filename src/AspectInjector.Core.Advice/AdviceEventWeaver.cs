using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.MethodCall
{
    internal class AdviceEventWeaver : AdviceInjectorBase<EventDefinition>
    {
        protected override void Weave(EventDefinition target, Advice effect, Injection injection)
        {
        }
    }
}