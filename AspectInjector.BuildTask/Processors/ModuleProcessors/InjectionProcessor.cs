﻿using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using AspectInjector.BuildTask.Validation;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

            //ValidateContexts(contexts);
        }

        private static bool CheckFilter(MethodDefinition targetMethod,
            string targetName,
            AspectDefinition aspectDefinition)
        {
            var result = true;

            var nameFilter = aspectDefinition.NameFilter;
            var accessModifierFilter = aspectDefinition.AccessModifierFilter;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                result = Regex.IsMatch(targetName, nameFilter);
            }

            if (result && accessModifierFilter != 0)
            {
                if (targetMethod.IsPrivate)
                {
                    result = (accessModifierFilter & AccessModifiers.Private) != 0;
                }
                else if (targetMethod.IsFamily)
                {
                    result = (accessModifierFilter & AccessModifiers.Protected) != 0;
                }
                else if (targetMethod.IsAssembly)
                {
                    result = (accessModifierFilter & AccessModifiers.Internal) != 0;
                }
                else if (targetMethod.IsFamilyOrAssembly)
                {
                    result = (accessModifierFilter & AccessModifiers.ProtectedInternal) != 0;
                }
                else if (targetMethod.IsPublic)
                {
                    result = (accessModifierFilter & AccessModifiers.Public) != 0;
                }
            }

            return result;
        }

        private List<AspectDefinition> FindAspectDefinitions(Collection<CustomAttribute> collection)
        {
            var result = collection.GetAttributesOfType<AspectAttribute>().Select(ParseAspectAttribute).ToList();

            var customAttrs = collection.GroupBy(ca => ca.AttributeType.Resolve().CustomAttributes.GetAttributeOfType<AspectDefinitionAttribute>()).Where(g => g.Key != null);

            result = result.Concat(customAttrs.Select(ca => ParseCustomAspectAttribute(ca.Key, ca.First()))).ToList();

            return result;
        }

        private AspectDefinition ParseAspectAttribute(CustomAttribute attr)
        {
            return new AspectDefinition()
            {
                AdviceClassType = ((TypeReference)attr.ConstructorArguments[0].Value).Resolve(),
                NameFilter = (string)attr.GetPropertyValue("NameFilter"),
                AccessModifierFilter = (AccessModifiers)(attr.GetPropertyValue("AccessModifierFilter") ?? 0),
                RoutableData = new List<CustomAttribute>()
            };
        }

        private AspectDefinition ParseCustomAspectAttribute(CustomAttribute attr, CustomAttribute baseAttr)
        {
            return new AspectDefinition()
            {
                AdviceClassType = ((TypeReference)attr.ConstructorArguments[0].Value).Resolve(),
                NameFilter = (string)attr.GetPropertyValue("NameFilter"),
                AccessModifierFilter = (AccessModifiers)(attr.GetPropertyValue("AccessModifierFilter") ?? 0),
                RoutableData = new List<CustomAttribute> { baseAttr }
            };
        }

        private void ProcessAspectDefinitions(MethodDefinition targetMethod, string targetName, IEnumerable<AspectDefinition> aspectDefinitions)
        {
            var contexts = aspectDefinitions
                .Where(def => CheckFilter(targetMethod, targetName, def))
                .GroupBy(d => d.AdviceClassType)
                .Select(g =>
                {
                    var adviceClassType = g.First().AdviceClassType;

                    return new AspectContext
                    {
                        TargetName = targetName,
                        TargetTypeContext = TypeContextFactory.GetOrCreateContext(targetMethod.DeclaringType),
                        AdviceClassType = adviceClassType,
                        AdviceClassScope = GetAspectScope(targetMethod, adviceClassType),
                        AspectRoutableData = g.SelectMany(d => d.RoutableData).ToArray()
                    };
                })
                .Where(ctx => _processors.Any(p => p.CanProcess(ctx.AdviceClassType)))
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

        private static AspectScope GetAspectScope(MethodDefinition targetMethod, TypeDefinition adviceClassType)
        {
            var customAttributes = adviceClassType.CustomAttributes;
            if (customAttributes.HasAttributeOfType<AspectScopeAttribute>())
                return (AspectScope)customAttributes.GetAttributeOfType<AspectScopeAttribute>().ConstructorArguments[0].Value;

            return targetMethod.IsStatic ? AspectScope.Type : AspectScope.Instance;
        }
    }
}