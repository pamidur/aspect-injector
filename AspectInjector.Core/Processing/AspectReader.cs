using AspectInjector.Broker;
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

namespace AspectInjector.Core.Processing
{
    public class AspectReader : IAspectReader
    {
        private ProcessingContext _context;
        protected ILogger Log { get; private set; }

        public void Init(ProcessingContext context)
        {
            _context = context;
            Log = context.Services.Log;
        }

        public IEnumerable<Aspect> ReadAspects(AssemblyDefinition assembly)
        {
            var aspects = Enumerable.Empty<Aspect>();
            aspects = aspects.Concat(ExtractAspectsFrom(assembly));

            foreach (var module in assembly.Modules)
            {
                aspects = aspects.Concat(ExtractAspectsFrom(module));

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

        protected virtual IEnumerable<Aspect> ExtractAspectsFrom<T>(T source) where T : class, ICustomAttributeProvider
        {
            //to much linq in this method, be careful!

            var aspectAndFilterPairs = source.CustomAttributes.Where(a => a.AttributeType.IsTypeOf(typeof(AspectAttribute))).Select(a => ParseAspectAttribute(source, a));

            var aspectDefinitions = source.CustomAttributes.GroupBy(ca => ca, ca => ca.AttributeType.Resolve().CustomAttributes.Where(ad => ad.AttributeType.IsTypeOf(typeof(AspectDefinitionAttribute)))).Where(g => g.Any());

            aspectAndFilterPairs = aspectAndFilterPairs.Concat(aspectDefinitions.SelectMany(g => g.SelectMany(ads => ads.Select(ad => ParseAspectAttribute(source, ad, g.Key)))));

            var result = aspectAndFilterPairs.Select(p => p.Item1).Concat(aspectAndFilterPairs.SelectMany(p => FindApplicableChildren(p.Item1, p.Item2)));

            return result;
        }

        private Tuple<Aspect<T>, ChildrenFilter> ParseAspectAttribute<T>(T source, CustomAttribute attr, CustomAttribute routableData = null)
            where T : class, ICustomAttributeProvider
        {
            return new Tuple<Aspect<T>, ChildrenFilter>(new Aspect<T>()
            {
                Target = source,
                InjectionHost = attr.GetConstructorValue<TypeReference>(0),
                RoutableData = routableData == null ? new List<CustomAttribute>() : new List<CustomAttribute> { routableData }
            },
            new ChildrenFilter
            {
                NameFilter = attr.GetPropertyValue<AspectAttributeBase, string>(a => a.NameFilter),
                AccessModifierFilter = attr.GetPropertyValue<AspectAttributeBase, AccessModifiers>(a => a.AccessModifierFilter),
            });
        }

        private Aspect MergeAspects(Aspect a1, Aspect a2)
        {
            a1.RoutableData = a1.RoutableData.Union(a2.RoutableData, CustomAttribureEqualityComparer.Instance).ToList();
            return a1;
        }

        private IEnumerable<Aspect> FindApplicableChildren<T>(Aspect<T> arg, ChildrenFilter filter) where T : class, ICustomAttributeProvider
        {
            // here goes applicable children lookup, below is exaple fot T : TypeDefinition looking gor applicable methods
            return Enumerable.Empty<Aspect>();
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

            if (result && accessModifierFilter != AccessModifiers.Any)
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
    }
}