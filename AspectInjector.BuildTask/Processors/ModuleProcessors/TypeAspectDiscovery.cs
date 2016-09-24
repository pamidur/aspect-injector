using System;
using System.Collections.Generic;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class TypeAspectDiscovery
    {
        private readonly List<FoundAspectDefinition> _allFoundDefinitions;

        public TypeAspectDiscovery(TypeDefinition typeDefinition, ModuleDefinition module)
        {
            _allFoundDefinitions = GetAspectDefinitions(module.CustomAttributes, d => new FoundAspectDefinition(d, FoundOn.Assembly))
                .Concat(GetDefinitionsForType(typeDefinition, FoundOn.Type))
                .Distinct()
                .ToList();
        }

        public IEnumerable<AspectDefinition> GetAspectDefinitions(MethodDefinition method, IList<CustomAttribute> memberAttributes)
        {
            return _allFoundDefinitions
                .Where(f => f.AppliesTo(method))
                .Select(f => f.AspectDefinition)
                .Concat(GetAspectDefinitions(memberAttributes, d => d));
        }

        private static IEnumerable<FoundAspectDefinition> GetDefinitionsForType(TypeDefinition typeDefinition, FoundOn foundOn)
        {
            return GetAspectDefinitions(typeDefinition.CustomAttributes, d => new FoundAspectDefinition(d, foundOn))
                .Concat(GetInterfaceAspects(typeDefinition))
                .Concat(GetInterfaceMemberAspects(typeDefinition))
                .Concat(GetBaseTypeDefinitions(typeDefinition));
        }

        private static IEnumerable<FoundAspectDefinition> GetBaseTypeDefinitions(TypeDefinition typeDefinition)
        {
            if (typeDefinition.BaseType == null)
                return Enumerable.Empty<FoundAspectDefinition>();

            var baseType = typeDefinition.BaseType.Resolve();
            return GetDefinitionsForType(baseType, FoundOn.BaseType)
                .Concat(GetMemberAspectDefinitions(baseType, FoundOn.BaseTypeMember));
        }

        private static IEnumerable<FoundAspectDefinition> GetInterfaceAspects(TypeDefinition type)
        {
            var parentDefinitions = type.Interfaces.SelectMany(i => GetInterfaceAspects(i.Resolve())).ToArray();

            return parentDefinitions
                .Concat(parentDefinitions.Select(p => new FoundAspectDefinition(p.AspectDefinition, FoundOn.Interface, type)))
                .Concat(type.IsInterface
                    ? GetAspectDefinitions(type.CustomAttributes, d => new FoundAspectDefinition(d, FoundOn.Interface, type))
                    : Enumerable.Empty<FoundAspectDefinition>());
        }

        private static IEnumerable<FoundAspectDefinition> GetInterfaceMemberAspects(TypeDefinition typeDefinition)
        {
            var ownDefinitions = typeDefinition.IsInterface
                ? GetMemberAspectDefinitions(typeDefinition, FoundOn.InterfaceMember)
                : Enumerable.Empty<FoundAspectDefinition>();

            return ownDefinitions.Concat(typeDefinition.Interfaces.Select(i => i.Resolve()).SelectMany(GetInterfaceMemberAspects));
        }

        private static IEnumerable<TResult> GetAspectDefinitions<TResult>(IList<CustomAttribute> attributes, Func<AspectDefinition, TResult> createResult)
        {
            return attributes.GetAttributesOfType<AspectAttribute>()
                             .Select(attr => createResult(new AspectDefinition(attr, null)))
                             .Concat(attributes
                                 .GroupBy(ca => ca.AttributeType.Resolve().CustomAttributes.GetAttributeOfType<AspectDefinitionAttribute>())
                                 .Where(g => g.Key != null)
                                 .Select(ca => createResult(new AspectDefinition(ca.Key, ca.First()))));
        }

        private static IEnumerable<FoundAspectDefinition> GetMemberAspectDefinitions(TypeDefinition type, FoundOn source)
        {
            return GetMethods(type).SelectMany(m => GetMemberAspectDefinitions(m, m.CustomAttributes, source))
                                   .Concat(type.Properties.SelectMany(p => GetMemberAspectDefinitions(p.GetMethod, p.CustomAttributes, source)))
                                   .Concat(type.Properties.SelectMany(p => GetMemberAspectDefinitions(p.SetMethod, p.CustomAttributes, source)))
                                   .Concat(type.Events.SelectMany(e => GetMemberAspectDefinitions(e.AddMethod, e.CustomAttributes, source)))
                                   .Concat(type.Events.SelectMany(e => GetMemberAspectDefinitions(e.RemoveMethod, e.CustomAttributes, source)));
        }

        private static IEnumerable<MethodDefinition> GetMethods(TypeDefinition type)
        {
            return type.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn);
        }

        private static IEnumerable<FoundAspectDefinition> GetMemberAspectDefinitions(MethodDefinition method, IList<CustomAttribute> attributes, FoundOn source)
        {
            return method == null
                ? Enumerable.Empty<FoundAspectDefinition>()
                : GetAspectDefinitions(attributes, d => new FoundAspectDefinition(d, source, method));
        }
    }
}