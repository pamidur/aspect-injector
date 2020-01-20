using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectInjector.Core.Services
{
    public class InjectionReader : IInjectionReader
    {
        private readonly IAspectReader _aspectReader;
        private readonly ILogger _log;

        public InjectionReader(IAspectReader aspectReader, ILogger logger)
        {
            _aspectReader = aspectReader;
            _log = logger;
        }

        public IReadOnlyCollection<InjectionDefinition> ReadAll(AssemblyDefinition assembly)
        {
            var injections = ExtractInjections(assembly);

            foreach (var module in assembly.Modules)
            {
                injections = injections.Concat(ExtractInjections(module));

                //todo:: sort types by base class 
                foreach (var type in module.GetTypes())
                {
                    injections = injections.Concat(ExtractInjections(type));

                    injections = injections.Concat(type.Events.SelectMany(ExtractInjections));
                    injections = injections.Concat(type.Properties.SelectMany(ExtractInjections));
                    //definitions = definitions.Concat(type.Fields.SelectMany(ExtractInjections));
                    injections = injections.Concat(type.Methods.SelectMany(ExtractInjections));
                }
            }

            injections = injections.GroupBy(a => a).Select(g => g.Aggregate(MergeInjections)).ToArray();

            //Remove trigger classes from injection
            var triggerTypes = new HashSet<TypeDefinition>(assembly.Modules.SelectMany(m => m.GetTypes())
                .Where(t => t.CustomAttributes.Any(a => a.AttributeType.FullName == WellKnownTypes.Injection)));

            return OrderByInheritance(injections.Where(d => !IsTriggerMember(d.Target, triggerTypes)))
                .ToArray();
        }

        protected virtual IEnumerable<InjectionDefinition> ExtractInjections(ICustomAttributeProvider target)
        {
            var injections = Enumerable.Empty<InjectionDefinition>();

            foreach (var trigger in target.CustomAttributes
                .Select(a => new { Attribute = a, Injections = FindInjections(a.AttributeType.Resolve()) })
                .Where(e => e.Injections.Count != 0).ToArray())
            {
                injections = injections.Concat(ParseInjectionAttribute(target, trigger.Attribute, trigger.Injections));
            }

            return injections;
        }

        protected static IReadOnlyList<CustomAttribute> FindInjections(TypeDefinition type)
        {
            var injections = new List<CustomAttribute>();

            injections.AddRange(type.CustomAttributes.Where(t => t.AttributeType.FullName == WellKnownTypes.Injection));

            if (type.BaseType != null && !type.BaseType.Match(StandardTypes.Attribute))
                injections.AddRange(FindInjections(type.BaseType.Resolve()).Where(a => a.GetPropertyValue<bool>(nameof(Injection.Inherited))).ToArray());

            return injections;
        }

        private IEnumerable<InjectionDefinition> ParseInjectionAttribute(ICustomAttributeProvider target, CustomAttribute trigger, IReadOnlyList<CustomAttribute> injectionAttrs)
        {
            var injections = Enumerable.Empty<InjectionDefinition>();

            foreach (var injectionAttr in injectionAttrs)
            {
                var aspectRef = injectionAttr.GetConstructorValue<TypeReference>(0);
                var propagation = injectionAttr.GetPropertyValue<PropagateTo>(nameof(Injection.Propagation));
                if (propagation == 0) propagation = PropagateTo.Members | PropagateTo.Types;

                if (propagation > PropagateTo.Everything)
                    _log.Log(GeneralRules.UnknownCompilationOption, trigger.AttributeType.Resolve(), GeneralRules.Literals.UnknownPropagationStrategy(propagation.ToString()));

                var propagationFilter = injectionAttr.GetPropertyValue<string>(nameof(Injection.PropagationFilter));
                Regex propagationRegex = null;
                if (propagationFilter != null)
                    try
                    {
                        propagationRegex = new Regex(propagationFilter, RegexOptions.CultureInvariant);
                    }
                    catch (Exception e)
                    {
                        if (propagation > PropagateTo.Everything)
                            _log.Log(GeneralRules.UnknownCompilationOption, trigger.AttributeType.Resolve(), GeneralRules.Literals.InvalidPropagationFilter(propagationFilter));
                    }

                var aspect = _aspectReader.Read(aspectRef.Resolve());

                if (aspect == null)
                {
                    _log.Log(InjectionRules.InjectionMustReferToAspect, target, aspectRef.Name);
                    continue;
                }

                var priority = injectionAttr.GetPropertyValue<ushort>(nameof(Injection.Priority));

                injections = injections.Concat(FindApplicableMembers(target, (aspect, priority, propagation, propagationRegex), trigger));
            }

            return injections;
        }

        private IEnumerable<InjectionDefinition> FindApplicableMembers(ICustomAttributeProvider target, (AspectDefinition aspect, ushort priority, PropagateTo propagation, Regex filter) injection, CustomAttribute trigger)
        {
            var result = Enumerable.Empty<InjectionDefinition>();

            Func<ICustomAttributeProvider, bool> additionalFilter = (provider) => !provider.IsCompilerGenerated();
            if ((injection.propagation & PropagateTo.IncludeCompilerGenerated) != 0)
                additionalFilter = (provider) => true;


            if (target is AssemblyDefinition assm)
                result = result.Concat(assm.Modules.SelectMany(nt => FindApplicableMembers(nt, injection, trigger)));

            if (target is ModuleDefinition module && (injection.propagation & PropagateTo.Types) != 0)
                result = result.Concat(module.Types.SelectMany(nt => FindApplicableMembers(nt, injection, trigger)));

            if (target is IMemberDefinition member && (injection.filter == null || injection.filter.IsMatch(member.Name)))
                result = result.Concat(CreateInjections(member, injection, trigger));

            if (target is TypeDefinition type)
            {
                if ((injection.propagation & PropagateTo.Methods) != 0)
                    result = result.Concat(type.Methods.Where(m => additionalFilter(m) && (m.IsNormalMethod() || m.IsConstructor)).SelectMany(m => FindApplicableMembers(m, injection, trigger)));
                if ((injection.propagation & PropagateTo.Events) != 0)
                    result = result.Concat(type.Events.Where(e => additionalFilter(e)).SelectMany(m => FindApplicableMembers(m, injection, trigger)));
                if ((injection.propagation & PropagateTo.Properties) != 0)
                    result = result.Concat(type.Properties.Where(p => additionalFilter(p)).SelectMany(m => FindApplicableMembers(m, injection, trigger)));
                if ((injection.propagation & PropagateTo.Types) != 0)
                    result = result.Concat(type.NestedTypes.Where(nt => additionalFilter(nt)).SelectMany(nt => FindApplicableMembers(nt, injection, trigger)));
            }

            return result;
        }

        private IEnumerable<InjectionDefinition> CreateInjections(IMemberDefinition target, (AspectDefinition aspect, ushort priority, PropagateTo propagation, Regex filter) injection, CustomAttribute trigger)
        {
            if (IsAspectMember(target)) return Enumerable.Empty<InjectionDefinition>();

            return injection.aspect.Effects.Where(e => e.IsApplicableFor(target)).Select(e => new InjectionDefinition()
            {
                Target = target,
                Source = injection.aspect,
                Priority = injection.priority,
                Effect = e,
                Triggers = new List<CustomAttribute> { trigger }
            });
        }

        private bool IsAspectMember(IMemberDefinition member)
        {
            if (member == null)
                return false;

            if (member is TypeDefinition type && _aspectReader.Read(type) != null)
                return true;

            return IsAspectMember(member.DeclaringType);
        }

        private bool IsTriggerMember(IMemberDefinition member, HashSet<TypeDefinition> triggerTypes)
        {
            if (member == null)
                return false;

            if (member is TypeDefinition type && triggerTypes.Contains(type))
                return true;

            return IsTriggerMember(member.DeclaringType, triggerTypes);
        }

        private InjectionDefinition MergeInjections(InjectionDefinition a1, InjectionDefinition a2)
        {
            a1.Priority = Enumerable.Max(new[] { a1.Priority, a2.Priority });
            a1.Triggers = a1.Triggers.Concat(a2.Triggers).Distinct().ToList();
            return a1;
        }

        private IEnumerable<InjectionDefinition> OrderByInheritance(IEnumerable<InjectionDefinition> injections)
        {
            //Target can be either a type or a method
            var types = new HashSet<TypeDefinition>(injections.Select(i => i.Target is TypeDefinition td ? td : i.Target.DeclaringType));
            while (types.Any())
            {
                var currentType = types.First();
                var baseType = currentType.BaseType?.Resolve();

                //While base type exists in the list of all affected types, set base class as current
                //This way we will process base classes first
                while (baseType != null && types.Contains(baseType))
                {
                    currentType = baseType;
                    baseType = currentType.BaseType.Resolve();
                }

                types.Remove(currentType);

                foreach (var currentInjection in injections.Where(i => i.Target == currentType || i.Target.DeclaringType == currentType))
                {
                    yield return currentInjection;
                }
            }
        }
    }
}