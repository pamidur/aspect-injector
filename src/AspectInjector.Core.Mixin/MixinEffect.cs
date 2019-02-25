using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class MixinEffect : Effect
    {
        public TypeReference InterfaceType { get; set; }

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            return target is TypeDefinition || target is PropertyDefinition || target is EventDefinition || target is MethodDefinition;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            if (!(effect is MixinEffect other))
                return false;

            return other.InterfaceType.Match(InterfaceType);
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            var result = true;

            if (!InterfaceType.Resolve().IsInterface)
            {
                log.Log(EffectRules.MixinSupportsOnlyInterfaces, aspect.Host, InterfaceType.Name);
                result = false;
            }

            if (!aspect.Host.Implements(InterfaceType))
            {
                log.Log(EffectRules.MixinSupportsOnlyAspectInterfaces, aspect.Host, aspect.Host.Name, InterfaceType.Name);
                result = false;
            }

            return result;
        }

        public override string ToString()
        {
            return $"Mixin::{InterfaceType.Name}";
        }
    }
}