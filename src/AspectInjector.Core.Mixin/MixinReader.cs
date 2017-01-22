using AspectInjector.Broker;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services.Abstract;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System;
using AspectInjector.Core.Contracts;

namespace AspectInjector.Core.Mixin
{
    internal class MixinReader : EffectExtractorBase<MixinEffect>
    {
        public MixinReader(string prefix, ILogger logger) : base(prefix, logger)
        {
        }

        protected override IEnumerable<MixinEffect> ReadEffects(CustomAttribute attribute)
        {
        }

        protected override IEnumerable<MixinEffect> ReadInjections(ModuleDefinition module)

        {
            var result = Enumerable.Empty<MixinEffect>();

            foreach (var type in module.Types)
                result = result.Concat(ReadMixins(type));

            result = Validate(result);

            return result;
        }

        private IEnumerable<MixinEffect> ReadMixins(TypeDefinition type)
        {
            var advices = from ca in type.CustomAttributes
                          where ca.AttributeType.IsTypeOf(typeof(Broker.Mixin))
                          select new MixinEffect { InterfaceType = ca.GetConstructorValue<TypeReference>(0), Aspect = type };

            foreach (var nestedType in type.NestedTypes)
                advices = advices.Concat(ReadMixins(nestedType));

            return advices;
        }

        private IEnumerable<MixinEffect> Validate(IEnumerable<MixinEffect> mixins)
        {
            foreach (var mixin in mixins)
            {
                if (!mixin.InterfaceType.Resolve().IsInterface)
                    Log.LogError(CompilationMessage.From($"{mixin.InterfaceType.Name} is not an interface.", mixin.Aspect.Resolve()));

                if (!mixin.Aspect.Implements(mixin.InterfaceType))
                    Log.LogError(CompilationMessage.From($"{mixin.Aspect.Name} should implement {mixin.InterfaceType.Name}.", mixin.Aspect.Resolve()));

                yield return mixin;
            }
        }
    }
}