using System.Collections.Generic;
using System.Linq;
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
            return module.Types
                         .Where(t => t.IsClass)
                         .SelectMany(t => t.GetClassesTree())
                         .SelectMany(c => GetAspectContexts(c, module));
        }

        private static IEnumerable<AspectContext> GetAspectContexts(TypeDefinition type, ModuleDefinition module)
        {
            return GetMembers(type).SelectMany(member => GetAspectContexts(member, new TypeAspectDiscovery(type, module)));
        }

        private static IEnumerable<IMemberDefinition> GetMembers(TypeDefinition type)
        {
            return type.Methods
                       .Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn)
                       .Cast<IMemberDefinition>()
                       .Concat(type.Properties)
                       .Concat(type.Events);
        }

        private static IEnumerable<AspectContext> GetAspectContexts(IMemberDefinition member, TypeAspectDiscovery typeAspects)
        {
            var methodDefinition = member as MethodDefinition;
            if (methodDefinition != null)
            {
                return CreateAspectContexts(methodDefinition, member.Name, typeAspects.GetAspectDefinitions(methodDefinition, member.CustomAttributes));
            }

            var propertyDefinition = member as PropertyDefinition;
            if (propertyDefinition != null)
            {
                return new[] { propertyDefinition.GetMethod, propertyDefinition.SetMethod }
                    .Where(m => m != null)
                    .SelectMany(m => CreateAspectContexts(m, member.Name, typeAspects.GetAspectDefinitions(m, member.CustomAttributes)));
            }

            var eventDefinition = member as EventDefinition;
            if (eventDefinition != null)
            {
                return CreateAspectContexts(eventDefinition.AddMethod, member.Name, typeAspects.GetAspectDefinitions(eventDefinition.AddMethod, member.CustomAttributes))
                    .Concat(CreateAspectContexts(eventDefinition.RemoveMethod, member.Name, typeAspects.GetAspectDefinitions(eventDefinition.RemoveMethod, member.CustomAttributes)));
            }

            return Enumerable.Empty<AspectContext>();
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