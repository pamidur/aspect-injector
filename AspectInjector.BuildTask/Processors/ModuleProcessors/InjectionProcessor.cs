using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
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
            foreach (var @class in module.Types.Where(t => t.IsClass))
            {
                var classAspectAttributes = @class.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();

                foreach (var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn).ToList())
                {
                    var methodAspectAttributes = method.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = MergeAspectAttributes(classAspectAttributes, methodAspectAttributes).ToList();

                    ProcessAspectAttributes(method, method.Name, allAspectAttributes);
                }

                foreach (var property in @class.Properties.ToList())
                {
                    var propertyAspectAttributes = property.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = MergeAspectAttributes(classAspectAttributes, propertyAspectAttributes).ToList();

                    if (property.GetMethod != null)
                    {
                        ProcessAspectAttributes(property.GetMethod, property.Name, allAspectAttributes);
                    }
                    if (property.SetMethod != null)
                    {
                        ProcessAspectAttributes(property.SetMethod, property.Name, allAspectAttributes);
                    }
                }

                foreach (var @event in @class.Events.ToList())
                {
                    var eventAspectAttributes = @event.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = MergeAspectAttributes(classAspectAttributes, eventAspectAttributes).ToList();

                    if (@event.AddMethod != null)
                    {
                        ProcessAspectAttributes(@event.AddMethod, @event.Name, allAspectAttributes);
                    }
                    if (@event.RemoveMethod != null)
                    {
                        ProcessAspectAttributes(@event.RemoveMethod, @event.Name, allAspectAttributes);
                    }
                }
            }

            //ValidateContexts(contexts);
        }

        private static bool CheckFilter(TargetMethodContext targetMethodContext,
            string targetName,
            CustomAttribute aspectAttribute)
        {
            var result = true;

            var targetMethod = targetMethodContext.TargetMethod;

            var nameFilter = (string)aspectAttribute.GetPropertyValue("NameFilter");
            object accessModifierFilterObject = aspectAttribute.GetPropertyValue("AccessModifierFilter");
            var accessModifierFilter = (AccessModifiers)(accessModifierFilterObject ?? 0);

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

        private static void SetAspectFactoryToContext(AspectContext context)
        {
            var aspectFactories = context.AspectType.Methods.Where(m => m.IsStatic && !m.IsConstructor && m.CustomAttributes.HasAttributeOfType<AspectFactoryAttribute>()).ToList();

            if (aspectFactories.Count > 1)
                throw new CompilationException("Only one method can be AspectFactory", aspectFactories.Last());

            if (aspectFactories.Count == 1)
            {
                context.AspectFactory = aspectFactories[0];
                context.AspectFactoryArgumentsSources = ProcessingUtils.GetAdviceArgumentsSources(context.AspectFactory).ToList();
            }
            else
            {
                var ctors = context.AspectType.Methods.Where(c => c.IsConstructor && !c.IsStatic && c.Parameters.Count == 0).ToList();

                if (ctors.Count == 0)
                    throw new CompilationException("Cannot find empty constructor for aspect. Create one or use AspectFactory", context.AspectType);

                context.AspectFactory = ctors.First();
                context.AspectFactoryArgumentsSources = new List<AdviceArgumentSource>();
            }
        }      

        private IEnumerable<CustomAttribute> MergeAspectAttributes(IEnumerable<CustomAttribute> classAttributes,
            IEnumerable<CustomAttribute> memberAttributes)
        {
            return classAttributes
                .Except(memberAttributes, new AspectAttributeEqualityComparer())
                .Union(memberAttributes);
        }

        private void ProcessAspectAttributes(MethodDefinition targetMethod,
            string targetName,
            IEnumerable<CustomAttribute> aspectAttributes)
        {
            var targetMethodContext = new TargetMethodContext(targetMethod);

            foreach (var attr in aspectAttributes.Where(a => CheckFilter(targetMethodContext, targetName, a)))
            {
                var context = new AspectContext()
                {
                    TargetMethodContext = targetMethodContext,
                    TargetName = targetName,
                    TargetType = targetMethod.DeclaringType,
                    AspectType = (TypeDefinition)attr.ConstructorArguments[0].Value,
                    //AspectCustomData = (object[])(a.GetPropertyValue("CustomData") ?? new object[] { })
                };

                SetAspectFactoryToContext(context);

                foreach (var processor in _processors)
                {
                    if (processor.CanProcess(context.AspectType))
                        processor.Process(context);
                }
            }
        }

        internal class AspectAttributeEqualityComparer : IEqualityComparer<CustomAttribute>
        {
            public bool Equals(CustomAttribute x, CustomAttribute y)
            {
                return object.Equals(x.ConstructorArguments, y.ConstructorArguments) &&
                    object.Equals(x.Properties, y.Properties);
            }

            public int GetHashCode(CustomAttribute obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}