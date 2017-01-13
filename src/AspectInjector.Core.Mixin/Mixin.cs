using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class Mixin : Effect
    {
        public TypeReference InterfaceType { get; set; }

        public override bool IsApplicableFor(CutDefinition cut)
        {
            return cut.Target is TypeDefinition;
        }

        public override bool Equals(Effect injection)
        {
            var other = (Mixin)injection;
            return other.InterfaceType.GetFQN() == InterfaceType.GetFQN();
        }

        public override int GetHashCode()
        {
            return InterfaceType.GetFQN().GetHashCode();
        }
    }
}