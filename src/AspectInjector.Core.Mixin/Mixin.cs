using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class Mixin : Injection
    {
        public TypeReference InterfaceType { get; set; }

        protected override bool IsApplicableForAspect(AspectUsage aspect)
        {
            return aspect.TargetKind == AspectTargetKind.TypeDefinition;
        }

        protected override bool IsEqualTo(Injection injection)
        {
            var other = (Mixin)injection;
            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }
    }
}