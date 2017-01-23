using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Processing.EqualityComparers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectInjector.Core.Services
{
    public class InjectionCollector : ServiceBase
    {
        private readonly AssetsCache _cache;

        public InjectionCollector(AssetsCache cache, Logger logger) : base(logger)
        {
            _cache = cache;
        }

        public IEnumerable<Injection> Collect(AssemblyDefinition assembly)
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
                    aspects = aspects.Concat(type.Methods.SelectMany(ExtractInjections));
                }
            }

            aspects = aspects.GroupBy(a => a).Select(g => g.Aggregate(MergeAspects)).ToList();

            return aspects;
        }

        protected virtual IEnumerable<Injection> ExtractInjections(ICustomAttributeProvider target)
        {
            var injections = Enumerable.Empty<Injection>();

            foreach (var attr in target.CustomAttributes.Where(a => a.AttributeType.IsTypeOf(typeof(Broker.Inject))))
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
                Log.LogError(CompilationMessage.From($"Aspect {aspectRef.FullName} should be an aspect class.", target));
                return Enumerable.Empty<Injection>();
            }

            if (aspectRef.HasGenericParameters)
                Log.LogError(CompilationMessage.From($"Aspect {aspectRef.FullName} should not have generic parameters.", target));

            var priority = attr.GetPropertyValue<Broker.Inject, ushort>(i => i.Priority);

            // var childFilter = attr.GetPropertyValue<Broker.Inject, InjectionChildFilter>(i => i.Filter);

            var injections = aspect.Effects.Where(e => e.IsApplicableFor(target)).Select(e => new Injection()
            {
                Target = target,
                Source = aspect,
                SourceReference = aspectRef,
                Priority = priority,
                Effect = e
            });

            //injections = injections.Concat (FindApplicableChildren(target, childFilter));

            return injections;
        }

        private Injection MergeAspects(Injection a1, Injection a2)
        {
            a1.Priority = Enumerable.Max(new[] { a1.Priority, a2.Priority });
            return a1;
        }

        //private IEnumerable<Models.Injection> FindApplicableChildren<T>(Injection<T> arg, ChildrenFilter filter) where T : class, ICustomAttributeProvider
        //{
        //    //todo:: here goes applicable children lookup, below is exaple fot T : TypeDefinition looking gor applicable methods
        //    return Enumerable.Empty<Models.Injection>();
        //}

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