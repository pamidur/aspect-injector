using System.Collections.Generic;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using AspectInjector.BuildTask.Validation;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class InjectionProcessor : IModuleProcessor
    {
        private readonly IEnumerable<IAspectProcessor> _processors;

        public InjectionProcessor(IEnumerable<IAspectProcessor> processors)
        {
            _processors = processors;
        }

        public void ProcessModule(ModuleDefinition module)
        {
            var contexts = GetAspectContexts(module).ToArray();

            Validator.ValidateAspectContexts(contexts);

            foreach (var context in contexts)
            {
                foreach (var processor in _processors.Where(processor => processor.CanProcess(context.AdviceClassType)))
                {
                    processor.Process(context);
                }
            }
        }

        private static IEnumerable<AspectContext> GetAspectContexts(ModuleDefinition module)
        {
            return module.Types
                         .Where(t => t.IsClass).SelectMany(t => t.GetClassesTree())
                         .SelectMany(GetAspectContexts);
        }

        private static IEnumerable<AspectContext> GetAspectContexts(TypeDefinition @class)
        {
            var classAspectDefinitions = GetAspectDefinitions(@class.CustomAttributes);

            return @class.Methods
                         .Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn)
                         .Cast<IMemberDefinition>()
                         .Concat(@class.Properties)
                         .Concat(@class.Events)
                         .SelectMany(member => GetAspectContexts(member, classAspectDefinitions));
        }

        private static IEnumerable<AspectContext> GetAspectContexts(IMemberDefinition member, IEnumerable<AspectDefinition> parentDefinitions)
        {
            var allAspects = parentDefinitions.Concat(GetAspectDefinitions(member.CustomAttributes)).ToArray();

            var methodDefinition = member as MethodDefinition;
            if (methodDefinition != null)
            {
                return CreateAspectContexts(methodDefinition, member.Name, allAspects);
            }

            var propertyDefinition = member as PropertyDefinition;
            if (propertyDefinition != null)
            {
                return CreateAspectContexts(propertyDefinition.GetMethod, member.Name, allAspects)
                    .Concat(CreateAspectContexts(propertyDefinition.SetMethod, member.Name, allAspects));
            }

            var eventDefinition = member as EventDefinition;
            if (eventDefinition != null)
            {
                return CreateAspectContexts(eventDefinition.AddMethod, member.Name, allAspects)
                    .Concat(CreateAspectContexts(eventDefinition.RemoveMethod, member.Name, allAspects));
            }

            return Enumerable.Empty<AspectContext>();
        }

        private static IEnumerable<AspectDefinition> GetAspectDefinitions(Collection<CustomAttribute> collection)
        {
            return collection.GetAttributesOfType<AspectAttribute>()
                             .Select(attr => new AspectDefinition(attr, null))
                             .Concat(collection
                                 .GroupBy(ca => ca.AttributeType.Resolve().CustomAttributes.GetAttributeOfType<AspectDefinitionAttribute>())
                                 .Where(g => g.Key != null)
                                 .Select(ca => new AspectDefinition(ca.Key, ca.First())));
        }

        private static IEnumerable<AspectContext> CreateAspectContexts(MethodDefinition targetMethod, string targetName, IEnumerable<AspectDefinition> definitions)
        {
            return definitions
                .Where(d => d.CanBeAppliedTo(targetMethod, targetName))
                .GroupBy(definition => definition.AdviceClassType)
                .Select(group => CreateAspectContext(targetMethod, targetName, @group));
        }

        private static AspectContext CreateAspectContext(MethodDefinition targetMethod, string targetName, IGrouping<TypeDefinition, AspectDefinition> @group)
        {
            var adviceClassType = @group.First().AdviceClassType;
            var routableData = @group.SelectMany(d => d.RoutableData).ToArray();
            return new AspectContext(targetMethod, targetName, adviceClassType, routableData);
        }
    }
}