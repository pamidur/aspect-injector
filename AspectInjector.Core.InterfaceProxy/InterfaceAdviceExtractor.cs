using AspectInjector.Broker;
using AspectInjector.Core.Dafaults;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.InterfaceProxy
{
    public class InterfaceAdviceExtractor : DefaultAdviceExtractorBase<InterfaceAdvice>
    {
        protected override IEnumerable<InterfaceAdvice> ExtractAdvices(ModuleDefinition module)

        {
            var result = Enumerable.Empty<InterfaceAdvice>();

            foreach (var type in module.Types)
                result = result.Concat(ExtractAdvicesFromType(type));

            return result;
        }

        private IEnumerable<InterfaceAdvice> ExtractAdvicesFromType(TypeDefinition type)
        {
            var advices = from ca in type.CustomAttributes
                          where ca.AttributeType.IsTypeOf(typeof(AdviceInterfaceProxyAttribute))
                          select new InterfaceAdvice { InterfaceType = (TypeReference)ca.ConstructorArguments[0].Value, HostType = type };

            foreach (var nestedType in type.NestedTypes)
                advices = advices.Concat(ExtractAdvicesFromType(nestedType));

            return advices;
        }
    }
}