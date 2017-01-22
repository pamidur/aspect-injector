using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class MixinEffect : Effect
    {
        public TypeReference InterfaceType { get; set; }

        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            return target is TypeDefinition;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            var other = effect as MixinEffect;

            if (effect == null)
                return false;

            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }
    }
}