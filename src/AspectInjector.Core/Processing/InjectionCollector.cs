using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Core.Processing.EqualityComparers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static AspectInjector.Broker.Cut;

namespace AspectInjector.Core.Processing
{
    public class InjectionCollector : IInjectionCollector
    {
        private ProcessingContext _context;
        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            Log = context.Services.Log;
        }

        public IEnumerable<Models.Injection> Collect(AssemblyDefinition assembly)
        {
            var aspects = Enumerable.Empty<Models.Injection>();
            aspects = aspects.Concat(ExtractAspectsFrom(assembly));

            foreach (var module in assembly.Modules)
            {
                aspects = aspects.Concat(ExtractAspectsFrom(module));

                var types = module.Types.SelectMany(t => t.GetTypesTree());

                foreach (var type in module.Types.SelectMany(t => t.GetTypesTree()))
                {
                    aspects = aspects.Concat(ExtractAspectsFrom(type));

                    aspects = aspects.Concat(type.GenericParameters.SelectMany(gp => ExtractAspectsFrom(gp)));

                    aspects = aspects.Concat(type.Events.SelectMany(e => ExtractAspectsFrom(e)));
                    aspects = aspects.Concat(type.Properties.SelectMany(p => ExtractAspectsFrom(p)));
                    aspects = aspects.Concat(type.Fields.SelectMany(f => ExtractAspectsFrom(f)));

                    foreach (var method in type.Methods)
                    {
                        aspects = aspects.Concat(ExtractAspectsFrom(method));
                        aspects = aspects.Concat(method.GenericParameters.SelectMany(gp => ExtractAspectsFrom(gp)));

                        aspects = aspects.Concat(method.Parameters.SelectMany(p => ExtractAspectsFrom(p)));

                        aspects = aspects.Concat(ExtractAspectsFrom(method.MethodReturnType));
                    }
                }
            }
            aspects = aspects.GroupBy(a => a).Select(g => g.Aggregate(MergeAspects)).ToList();

            return aspects;
        }

        protected virtual IEnumerable<Models.Injection> ExtractAspectsFrom<T>(T source) where T : class, ICustomAttributeProvider
        {
            //to much linq in this method, be careful!

            var aspectAndFilterPairs = source.CustomAttributes.Where(a => a.AttributeType.IsTypeOf(typeof(Broker.Cut))).Select(a => ParseAspectAttribute(source, a));

            var aspectDefinitions = source.CustomAttributes.GroupBy(ca => ca, ca => ca.AttributeType.Resolve().CustomAttributes.Where(ad => ad.AttributeType.IsTypeOf(typeof(Broker.CutSpecification)))).Where(g => g.Any());

            aspectAndFilterPairs = aspectAndFilterPairs.Concat(aspectDefinitions.SelectMany(g => g.SelectMany(ads => ads.Select(ad => ParseAspectAttribute(source, ad, g.Key)))));

            var result = aspectAndFilterPairs.SelectMany(p => FindApplicableChildren(p.Item1, p.Item2).Concat(new[] { p.Item1 }));

            return result;
        }

        private Tuple<Injection<T>, ChildrenFilter> ParseAspectAttribute<T>(T source, CustomAttribute attr, CustomAttribute routableData = null)
            where T : class, ICustomAttributeProvider
        {
            var injectionHost = attr.GetConstructorValue<TypeReference>(0).Resolve();

            if (!injectionHost.IsClass)
                Log.LogError(CompilationError.From($"Aspect {injectionHost.FullName} should be a class.", source));

            if (injectionHost.HasGenericParameters)
                Log.LogError(CompilationError.From($"Aspect {injectionHost.FullName} should not have generic parameters.", source));

            var aspectFactory = injectionHost.Methods
                .Where(c => c.IsConstructor && !c.IsStatic && !c.Parameters.Any())
                .FirstOrDefault();

            var aspectFactories = injectionHost.Methods.Where(m => m.IsStatic && !m.IsConstructor && m.CustomAttributes.Any(ca => ca.AttributeType.IsTypeOf(typeof(Broker.AspectFactory)))).ToList();

            foreach (var af in aspectFactories)
            {
                if (aspectFactories.Count > 1)
                    Log.LogError(CompilationError.From("Cannot have aspect factories.", af));

                if (af.Parameters.Any())
                    Log.LogError(CompilationError.From("Aspect factory cannot have parameters.", af));

                if (af.GenericParameters.Any())
                    Log.LogError(CompilationError.From("Aspect factory cannot have generic parameters.", af));
            }

            if (aspectFactories.Any())
                aspectFactory = aspectFactories.First();

            return new Tuple<Injection<T>, ChildrenFilter>(new Injection<T>()
            {
                Target = source,
                Source = injectionHost,
                AspectFactory = aspectFactory,
                RoutableData = routableData == null ? new List<CustomAttribute>() : new List<CustomAttribute> { routableData }
            },
            null//attr.GetPropertyValue<AspectBase, ChildrenFilter>(a => a.Filter)
            );
        }

        private Injection MergeAspects(Models.Injection a1, Models.Injection a2)
        {
            a1.RoutableData = a1.RoutableData.Union(a2.RoutableData, CustomAttribureEqualityComparer.Instance).ToList();
            return a1;
        }

        private IEnumerable<Models.Injection> FindApplicableChildren<T>(Injection<T> arg, ChildrenFilter filter) where T : class, ICustomAttributeProvider
        {
            //todo:: here goes applicable children lookup, below is exaple fot T : TypeDefinition looking gor applicable methods
            return Enumerable.Empty<Models.Injection>();
        }

        private static bool CheckFilter(MethodDefinition targetMethod,
            string targetName,
            ChildrenFilter aspectDefinition)
        {
            var result = true;

            var nameFilter = aspectDefinition.NameFilter;
            var accessModifierFilter = aspectDefinition.AccessModifierFilter;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                result = Regex.IsMatch(targetName, nameFilter);
            }

            if (result && accessModifierFilter != AccessModifier.Any)
            {
                if (targetMethod.IsPrivate)
                {
                    result = (accessModifierFilter & AccessModifier.Private) != 0;
                }
                else if (targetMethod.IsFamily)
                {
                    result = (accessModifierFilter & AccessModifier.Protected) != 0;
                }
                else if (targetMethod.IsAssembly)
                {
                    result = (accessModifierFilter & AccessModifier.Internal) != 0;
                }
                else if (targetMethod.IsFamilyOrAssembly)
                {
                    result = (accessModifierFilter & AccessModifier.ProtectedInternal) != 0;
                }
                else if (targetMethod.IsPublic)
                {
                    result = (accessModifierFilter & AccessModifier.Public) != 0;
                }
            }

            return result;
        }
    }
}