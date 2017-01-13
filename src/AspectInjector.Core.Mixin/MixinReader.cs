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
    internal class MixinReader : EffectExtractorBase<Mixin>
    {
        public MixinReader(string prefix, ILogger logger) : base(prefix, logger)
        {
        }

        protected override IEnumerable<Mixin> ReadEffects(CustomAttribute attribute)
        {
        }

        protected override IEnumerable<Mixin> ReadInjections(ModuleDefinition module)

        {
            var result = Enumerable.Empty<Mixin>();

            foreach (var type in module.Types)
                result = result.Concat(ReadMixins(type));

            result = Validate(result);

            return result;
        }

        private IEnumerable<Mixin> ReadMixins(TypeDefinition type)
        {
            var advices = from ca in type.CustomAttributes
                          where ca.AttributeType.IsTypeOf(typeof(Broker.Mixin))
                          select new Mixin { InterfaceType = ca.GetConstructorValue<TypeReference>(0), Aspect = type };

            foreach (var nestedType in type.NestedTypes)
                advices = advices.Concat(ReadMixins(nestedType));

            return advices;
        }

        private IEnumerable<Mixin> Validate(IEnumerable<Mixin> mixins)
        {
            foreach (var mixin in mixins)
            {
                if (!mixin.InterfaceType.Resolve().IsInterface)
                    Log.LogError(CompilationError.From($"{mixin.InterfaceType.Name} is not an interface.", mixin.Aspect.Resolve()));

                if (!mixin.Aspect.Implements(mixin.InterfaceType))
                    Log.LogError(CompilationError.From($"{mixin.Aspect.Name} should implement {mixin.InterfaceType.Name}.", mixin.Aspect.Resolve()));

                yield return mixin;
            }
        }
    }
}