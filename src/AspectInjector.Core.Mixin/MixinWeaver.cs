using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    public class MixinWeaver : IEffectWeaver
    {
        public byte Priority => 10;

        public void Weave(InjectionDefinition injection)
        {
            var process = new MixinWeaveProcess((TypeDefinition)injection.Target, injection.Source, (MixinEffect)injection.Effect);
            process.Execute();
        }

        public bool CanWeave(InjectionDefinition injection)
        {
            return injection.Target is TypeDefinition && injection.Effect is MixinEffect;
        }
    }
}