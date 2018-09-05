using System;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Mixin
{
    internal class MixinEffect : Effect
    {
        public TypeReference InterfaceType { get; set; }

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            return target is TypeDefinition;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            var other = effect as MixinEffect;

            if (effect == null)
                return false;

            return other.InterfaceType.Match(InterfaceType);
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (!InterfaceType.Resolve().IsInterface)
            {
                log.LogError(CompilationMessage.From($"{InterfaceType.FullName} is not an interface.", aspect.Host));
                return false;
            }

            if (!aspect.Host.Implements(InterfaceType))
            {
                log.LogError(CompilationMessage.From($"{aspect.Host.FullName} should implement {InterfaceType.FullName}.", aspect.Host));
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"Mixin::{InterfaceType.Name}";
        }
    }
}