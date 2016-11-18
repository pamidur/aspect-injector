using AspectInjector.Broker;
using AspectInjector.Core.Defaults;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.InterfaceProxy
{
    internal class InterfaceAdviceExtractor : DefaultAdviceExtractorBase<InterfaceAdvice>
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
                          select new InterfaceAdvice { InterfaceType = ca.GetConstructorValue<TypeReference>(0), HostType = type };

            foreach (var nestedType in type.NestedTypes)
                advices = advices.Concat(ExtractAdvicesFromType(nestedType));

            return advices;
        }

        //private InterfaceAdvice ValidateAdvice(InterfaceAdvice advice)
        //{
        //    if (!advice.InterfaceType.Resolve().IsInterface)
        //        throw new CompilationException(context.InterfaceDefinition.Name + " is not an interface on interface injection definition on acpect " + aspectDefinition.Name, aspectDefinition);

        //    if (!context.AspectContext.AdviceClassType.ImplementsInterface(context.InterfaceDefinition))
        //        throw new CompilationException(aspectDefinition.Name + " should implement " + interfaceDefinition.Name, aspectDefinition);
        //}
    }
}