using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AspectInjector.Core.Services
{
    public class InjectionCollector : IInjectionCollector
    {
        private readonly IAssetsCache _cache;
        private readonly ILogger _log;

        public InjectionCollector(IAssetsCache cache, ILogger logger)
        {
            _cache = cache;
            _log = logger;
        }

        public IReadOnlyCollection<Injection> Collect(AssemblyDefinition assembly)
        {
            var aspects = ExtractInjections(assembly);

            foreach (var module in assembly.Modules)
            {
                aspects = aspects.Concat(ExtractInjections(module));

                foreach (var type in module.GetTypes())
                {
                    aspects = aspects.Concat(ExtractInjections(type));

                    aspects = aspects.Concat(type.Events.SelectMany(ExtractInjections));
                    aspects = aspects.Concat(type.Properties.SelectMany(ExtractInjections));
                    //aspects = aspects.Concat(type.Fields.SelectMany(ExtractInjections));
                    aspects = aspects.Concat(type.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsRemoveOn && !m.IsAddOn).SelectMany(ExtractInjections));
                }
            }

            aspects = aspects.GroupBy(a => a).Select(g => g.Aggregate(MergeInjections)).ToList();

            return aspects.ToList();
        }

        protected virtual IEnumerable<Injection> ExtractInjections(ICustomAttributeProvider target)
        {
            var injections = Enumerable.Empty<Injection>();

            foreach (var attr in target.CustomAttributes.Where(a => a.AttributeType.IsTypeOf(typeof(Broker.Inject))).ToList())
            {
                injections = injections.Concat(ParseAspectAttribute(target, attr));
                target.CustomAttributes.Remove(attr);
            }

            return injections;
        }

        private IEnumerable<Injection> ParseAspectAttribute(ICustomAttributeProvider target, CustomAttribute attr)
        {
            var aspectRef = attr.GetConstructorValue<TypeReference>(0);
            var aspect = _cache.ReadAspect(aspectRef.Resolve());

            if (aspect == null)
            {
                _log.LogError(CompilationMessage.From($"Type {aspectRef.FullName} should be an aspect class.", target));
                return Enumerable.Empty<Injection>();
            }

            var priority = attr.GetPropertyValue<Broker.Inject, ushort>(i => i.Priority);

            // var childFilter = attr.GetPropertyValue<Broker.Inject, InjectionChildFilter>(i => i.Filter);

            var injections = CreateInjections(target, aspect, priority);

            injections = injections.Concat(FindApplicableChildren(target, aspect, priority/*, childFilter*/));

            return injections;
        }

        private IEnumerable<Injection> FindApplicableChildren(ICustomAttributeProvider target, AspectDefinition aspect, ushort priority)
        {
            var result = Enumerable.Empty<Injection>();

            var type = target as TypeDefinition;

            if (type != null)
            {
                result = result.Concat(type.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsRemoveOn && !m.IsAddOn)
                    .SelectMany(m => CreateInjections(m, aspect, priority)));
                result = result.Concat(type.Events.SelectMany(m => CreateInjections(m, aspect, priority)));
                result = result.Concat(type.Properties.SelectMany(m => CreateInjections(m, aspect, priority)));
            }

            return result;
        }

        private IEnumerable<Injection> CreateInjections(ICustomAttributeProvider target, AspectDefinition aspect, ushort priority)
        {
            return aspect.Effects.Where(e => e.IsApplicableFor(target)).Select(e => new Injection()
            {
                Target = target,
                Source = aspect,
                Priority = priority,
                Effect = e
            });
        }

        private Injection MergeInjections(Injection a1, Injection a2)
        {
            a1.Priority = Enumerable.Max(new[] { a1.Priority, a2.Priority });
            return a1;
        }

        //private static bool CheckFilter(MethodDefinition targetMethod,
        //    string targetName,
        //    ChildrenFilter aspectDefinition)
        //{
        //    var result = true;

        //    var nameFilter = aspectDefinition.NameFilter;
        //    var accessModifierFilter = aspectDefinition.AccessModifierFilter;

        //    if (!string.IsNullOrEmpty(nameFilter))
        //    {
        //        result = Regex.IsMatch(targetName, nameFilter);
        //    }

        //    if (result && accessModifierFilter != AccessModifier.Any)
        //    {
        //        if (targetMethod.IsPrivate)
        //        {
        //            result = (accessModifierFilter & AccessModifier.Private) != 0;
        //        }
        //        else if (targetMethod.IsFamily)
        //        {
        //            result = (accessModifierFilter & AccessModifier.Protected) != 0;
        //        }
        //        else if (targetMethod.IsAssembly)
        //        {
        //            result = (accessModifierFilter & AccessModifier.Internal) != 0;
        //        }
        //        else if (targetMethod.IsFamilyOrAssembly)
        //        {
        //            result = (accessModifierFilter & AccessModifier.ProtectedInternal) != 0;
        //        }
        //        else if (targetMethod.IsPublic)
        //        {
        //            result = (accessModifierFilter & AccessModifier.Public) != 0;
        //        }
        //    }

        //    return result;
        //}
    }
}