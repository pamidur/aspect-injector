using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinExtractor : EffectExtractorBase<TypeDefinition, MixinEffect>
    {
        public MixinExtractor(ILogger logger) : base(logger)
        {
        }

        protected override IEnumerable<MixinEffect> Extract(TypeDefinition type)
        {
            var advices = from ca in type.CustomAttributes
                          where ca.AttributeType.IsTypeOf(typeof(Broker.Mixin))
                          select new MixinEffect { InterfaceType = ca.GetConstructorValue<TypeReference>(0) };

            return advices;
        }

        private IEnumerable<MixinEffect> Validate(IEnumerable<MixinEffect> mixins)
        {
            foreach (var mixin in mixins)
            {
                if (!mixin.InterfaceType.Resolve().IsInterface)
                    _log.LogError(CompilationMessage.From($"{mixin.InterfaceType.Name} is not an interface.", mixin.Aspect.Resolve()));

                if (!mixin.Aspect.Implements(mixin.InterfaceType))
                    _log.LogError(CompilationMessage.From($"{mixin.Aspect.Name} should implement {mixin.InterfaceType.Name}.", mixin.Aspect.Resolve()));

                yield return mixin;
            }
        }
    }
}