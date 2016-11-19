using AspectInjector.Broker;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinReader : InjectionReaderBase<Mixin>
    {
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
                          where ca.AttributeType.IsTypeOf(typeof(AdviceInterfaceProxyAttribute))
                          select new Mixin { InterfaceType = ca.GetConstructorValue<TypeReference>(0), HostType = type };

            foreach (var nestedType in type.NestedTypes)
                advices = advices.Concat(ReadMixins(nestedType));

            return advices;
        }

        private IEnumerable<Mixin> Validate(IEnumerable<Mixin> mixins)
        {
            foreach (var mixin in mixins)
            {
                if (!mixin.InterfaceType.Resolve().IsInterface)
                    Log.LogError(new CompilationError($"{mixin.InterfaceType.Name} is not an interface.", mixin.HostType));

                if (!mixin.HostType.Implements(mixin.InterfaceType))
                    Log.LogError(new CompilationError($"{mixin.HostType.Name} should implement {mixin.InterfaceType.Name}.", mixin.HostType));

                yield return mixin;
            }
        }
    }
}