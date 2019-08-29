using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
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
            var aspects = ExtractInjections(assembly);

            foreach (var module in assembly.Modules)
            {
                aspects = aspects.Concat(ExtractInjections(module));

                //todo:: sort types by base class 
                foreach (var type in module.GetTypes())
                {
                    aspects = aspects.Concat(ExtractInjections(type));

                    aspects = aspects.Concat(type.Events.SelectMany(ExtractInjections));
                    aspects = aspects.Concat(type.Properties.SelectMany(ExtractInjections));
                    //aspects = aspects.Concat(type.Fields.SelectMany(ExtractInjections));
                    aspects = aspects.Concat(type.Methods.SelectMany(ExtractInjections));
                }
            }

            aspects = aspects.GroupBy(a => a).Select(g => g.Aggregate(MergeInjections)).ToList();

            return aspects.ToList();
        }

        protected virtual IEnumerable<InjectionDefinition> ExtractInjections(ICustomAttributeProvider target)
        {
            var injections = Enumerable.Empty<InjectionDefinition>();

            foreach (var trigger in target.CustomAttributes
                .Select(a => new { attribute = a, injections = a.AttributeType.Resolve().CustomAttributes.Where(t => t.AttributeType.FullName == WellKnownTypes.Injection).ToArray() })
                .Where(e => e.injections.Length != 0).ToArray())
                injections = injections.Concat(ParseInjectionAttribute(target, trigger.attribute, trigger.injections));


            return injections;
        }

        private IEnumerable<InjectionDefinition> ParseInjectionAttribute(ICustomAttributeProvider target, CustomAttribute trigger, CustomAttribute[] injectionAttrs)
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
                    result = result.Concat(type.Properties.Where(p=>additionalFilter(p)).SelectMany(m => FindApplicableMembers(m, injection, trigger)));
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

        private InjectionDefinition MergeInjections(InjectionDefinition a1, InjectionDefinition a2)
        {
            a1.Priority = Enumerable.Max(new[] { a1.Priority, a2.Priority });
            a1.Triggers = a1.Triggers.Concat(a2.Triggers).Distinct().ToList();
            return a1;
        }
    }
}