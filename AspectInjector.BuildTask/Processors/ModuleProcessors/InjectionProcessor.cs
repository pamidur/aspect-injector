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
            foreach (var @class in module.Types.Where(t => t.IsClass).SelectMany(t => t.GetClassesTree()))
            {
                var classAspectDefinitions = FindAspectDefinitions(@class.CustomAttributes);

                foreach (var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn).ToList())
                {
                    var methodAspectDefinitions = FindAspectDefinitions(method.CustomAttributes);
                    ProcessAspectDefinitions(method, method.Name, classAspectDefinitions.Concat(methodAspectDefinitions));
                }

                foreach (var property in @class.Properties.ToList())
                {
                    var propertyAspectDefinitions = FindAspectDefinitions(property.CustomAttributes);
                    var allAspectDefinitions = classAspectDefinitions.Concat(propertyAspectDefinitions);

                    if (property.GetMethod != null)
                    {
                        ProcessAspectDefinitions(property.GetMethod, property.Name, allAspectDefinitions);
                    }

                    if (property.SetMethod != null)
                    {
                        ProcessAspectDefinitions(property.SetMethod, property.Name, allAspectDefinitions);
                    }
                }

                foreach (var @event in @class.Events.ToList())
                {
                    var eventAspectDefinitions = FindAspectDefinitions(@event.CustomAttributes);
                    var allAspectDefinitions = classAspectDefinitions.Concat(eventAspectDefinitions);

                    if (@event.AddMethod != null)
                    {
                        ProcessAspectDefinitions(@event.AddMethod, @event.Name, allAspectDefinitions);
                    }

                    if (@event.RemoveMethod != null)
                    {
                        ProcessAspectDefinitions(@event.RemoveMethod, @event.Name, allAspectDefinitions);
                    }
                }
            }
        }

        private static List<AspectDefinition> FindAspectDefinitions(Collection<CustomAttribute> collection)
        {
            var customAttrs = collection
                .GroupBy(ca => ca.AttributeType.Resolve().CustomAttributes.GetAttributeOfType<AspectDefinitionAttribute>())
                .Where(g => g.Key != null);

            var result = collection.GetAttributesOfType<AspectAttribute>()
                .Select(attr => new AspectDefinition(attr, null))
                .Concat(customAttrs.Select(ca => new AspectDefinition(ca.Key, ca.First())))
                .ToList();

            return result;
        }

        private static AspectContext CreateAspectContext(MethodDefinition targetMethod, string targetName, IGrouping<TypeDefinition, AspectDefinition> @group)
        {
            var adviceClassType = @group.First().AdviceClassType;
            var routableData = @group.SelectMany(d => d.RoutableData).ToArray();
            return new AspectContext(targetMethod, targetName, adviceClassType, routableData);
        }

        private void ProcessAspectDefinitions(MethodDefinition targetMethod, string targetName, IEnumerable<AspectDefinition> aspectDefinitions)
        {
            var contexts = aspectDefinitions
                .Where(definition => definition.CanBeAppliedTo(targetMethod, targetName))
                .GroupBy(definition => definition.AdviceClassType)
                .Select(group => CreateAspectContext(targetMethod, targetName, group))
                .Where(context => _processors.Any(p => p.CanProcess(context.AdviceClassType)))
                .ToList();

            Validator.ValidateAspectContexts(contexts);

            foreach (var context in contexts)
            {
                // setting the TargetMethodContext here for better performance
                context.TargetMethodContext = MethodContextFactory.GetOrCreateContext(targetMethod);

                foreach (var processor in _processors)
                    if (processor.CanProcess(context.AdviceClassType))
                        processor.Process(context);
            }
        }
    }
}