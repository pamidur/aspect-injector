using System.Collections.Generic;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using AspectInjector.BuildTask.Validation;
using Mono.Cecil;

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

        internal static IEnumerable<AspectContext> GetAspectContexts(ModuleDefinition module)
        {
            var assemblyAspectDefinitions = GetAspectDefinitions(module.CustomAttributes).ToArray();

            return module.Types
                         .Where(t => t.IsClass).SelectMany(t => t.GetClassesTree())
                         .SelectMany(definition => GetAspectContexts(definition, assemblyAspectDefinitions));
        }

        private static IEnumerable<AspectContext> GetAspectContexts(TypeDefinition @class, IEnumerable<AspectDefinition> parentDefinitions)
        {
            var allAspects = GetAspectDefinitions(GetCustomAttributeRecursive(@class).ToList(), parentDefinitions);

            return GetTypeMembers(@class)
                .Concat(GetInterfacesRecursive(@class).Distinct().SelectMany(i => GetTypeMembers(i.Resolve())))
                .SelectMany(member => GetAspectContexts(member, allAspects));
        }

        private static IEnumerable<CustomAttribute> GetCustomAttributeRecursive(TypeDefinition type)
        {
            var baseTypesAndInterfaces = GetBaseTypesAndInterfaces(type);
            return type.CustomAttributes.Concat(baseTypesAndInterfaces.SelectMany(GetCustomAttributeRecursive));
        }

        private static TypeDefinition[] GetBaseTypesAndInterfaces(TypeDefinition type)
        {
            return type.Interfaces.Concat(new[] { type.BaseType })
                       .Where(t => t != null)
                       .Select(t => t.Resolve())
                       .ToArray();
        } 

        private static IEnumerable<TypeReference> GetInterfacesRecursive(TypeDefinition type)
        {
            return type.Interfaces.Concat(type.Interfaces.SelectMany(i => GetInterfacesRecursive(i.Resolve())));
        } 

        private static IEnumerable<IMemberDefinition> GetTypeMembers(TypeDefinition type)
        {
            return type.Methods
                       .Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn)
                       .Cast<IMemberDefinition>()
                       .Concat(type.Properties)
                       .Concat(type.Events);
        }

        private static IEnumerable<AspectContext> GetAspectContexts(IMemberDefinition member, IEnumerable<AspectDefinition> parentDefinitions)
        {
            var allAspects = GetAspectDefinitions(member.CustomAttributes, parentDefinitions);

            var methodDefinition = member as MethodDefinition;
            if (methodDefinition != null)
            {
                return CreateAspectContexts(methodDefinition, member.Name, allAspects);
            }

            var propertyDefinition = member as PropertyDefinition;
            if (propertyDefinition != null)
            {
                return new[] { propertyDefinition.GetMethod, propertyDefinition.SetMethod }
                    .Where(m => m != null)
                    .SelectMany(m => CreateAspectContexts(m, member.Name, allAspects));
            }

            var eventDefinition = member as EventDefinition;
            if (eventDefinition != null)
            {
                return CreateAspectContexts(eventDefinition.AddMethod, member.Name, allAspects)
                    .Concat(CreateAspectContexts(eventDefinition.RemoveMethod, member.Name, allAspects));
            }

            return Enumerable.Empty<AspectContext>();
        }

        private static AspectDefinition[] GetAspectDefinitions(IList<CustomAttribute> attributes, IEnumerable<AspectDefinition> parentDefinitions)
        {
            return parentDefinitions.Concat(GetAspectDefinitions(attributes)).ToArray();
        }

        private static IEnumerable<AspectDefinition> GetAspectDefinitions(IList<CustomAttribute> collection)
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
                .Select(group => new AspectContext(targetMethod, targetName, @group.Key, @group.SelectMany(d => d.RoutableData).ToArray()));
        }
    }
}