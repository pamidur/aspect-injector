using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class Mixin : Effect
    {
        public TypeReference InterfaceType { get; set; }

        protected override bool IsApplicableForAspect(Injection aspect)
        {
            return aspect.TargetKind == InjectionTargetType.TypeDefinition;
        }

        protected override bool IsEqualTo(Effect injection)
        {
            var other = (Mixin)injection;
            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }
    }
}