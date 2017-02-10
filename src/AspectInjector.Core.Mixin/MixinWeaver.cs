using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    public class MixinWeaver : IEffectWeaver
    {
        public byte Priority => 10;

        public void Weave(Injection injection)
        {
            var process = new MixinWeaveProcess((TypeDefinition)injection.Target, injection.Source, (MixinEffect)injection.Effect);
            process.Execute();
        }

        public bool CanWeave(Injection injection)
        {
            return injection.Target is TypeDefinition && injection.Effect is MixinEffect;
        }
    }
}