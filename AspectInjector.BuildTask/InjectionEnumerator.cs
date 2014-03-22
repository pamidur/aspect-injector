using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal static class InjectionEnumerator
    {
        public static ModuleDefinition ForEachInjection(this ModuleDefinition module, 
            Action<InjectionContext> injectionCallback)
        {
            foreach (var @class in module.Types.Where(t => t.IsClass))
            {
                var classAspectAttributes = @class.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();

                foreach (var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn))
                {
                    var methodAspectAttributes = method.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    ProcessAspects(method, method.Name, classAspectAttributes.Union(methodAspectAttributes), injectionCallback);
                }

                foreach (var property in @class.Properties)
                {
                    var propertyAspectAttributes = property.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = propertyAspectAttributes.Union(classAspectAttributes).ToList();

                    if (property.GetMethod != null)
                    {
                        ProcessAspects(property.GetMethod, property.Name, allAspectAttributes, injectionCallback);
                    }
                    if (property.SetMethod != null)
                    {
                        ProcessAspects(property.SetMethod, property.Name, allAspectAttributes, injectionCallback);
                    }
                }
            }

            return module;
        }

        private static void ProcessAspects(MethodDefinition targetMethod, 
            string targetName, 
            IEnumerable<CustomAttribute> aspectAttributes,
            Action<InjectionContext> injectionCallback)
        {
            aspectAttributes.Where(a => CheckFilter(targetMethod, targetName, a))
                .Select(a => new InjectionContext() 
                { 
                    TargetMethod = targetMethod, 
                    TargetName = targetName, 
                    AspectType = (TypeDefinition)a.ConstructorArguments[0].Value 
                })
                .ToList()
                .ForEach(c => ProcessAdvices(c, injectionCallback));
        }

        private static void ProcessAdvices(InjectionContext parentContext,
            Action<InjectionContext> injectionCallback)
        {
            foreach (var adviceMethod in GetAdviceMethods(parentContext.AspectType))
            {
                ProcessAdvice(adviceMethod, injectionCallback, parentContext);
            }
        }

        private static void ProcessAdvice(MethodDefinition adviceMethod,
            Action<InjectionContext> injectionCallback,
            InjectionContext parentContext)
        {
            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            var points = (InjectionPoints)adviceAttribute.ConstructorArguments[0].Value;
            var targets = (InjectionTargets)adviceAttribute.ConstructorArguments[1].Value;

            if (CheckTarget(parentContext.TargetMethod, targets))
            {
                var context = new InjectionContext(parentContext);

                context.AdviceMethod = adviceMethod;
                context.AdviceArgumentsSources = GetAdviceArgumentsSources(adviceMethod).ToList();

                if ((points & InjectionPoints.Before) != 0)
                {
                    if (context.IsAbortable && !context.AdviceMethod.ReturnType.IsTypeOf(adviceMethod.ReturnType))
                        throw new CompilationException("Return types of advice (" + adviceMethod.FullName + ") and target (" + context.TargetMethod.FullName + ") should be the same", context.TargetMethod);

                    if (!context.IsAbortable && !adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionPoints.Before without argument of AdviceArgumentSource.AbortFlag can be System.Void only", adviceMethod);

                    context.InjectionPoint = InjectionPoints.Before;
                }

                if ((points & InjectionPoints.After) != 0)
                {
                    if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", adviceMethod);

                    if (context.IsAbortable)
                        throw new CompilationException("Method should have a return value and inject into InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", adviceMethod);

                    context.InjectionPoint = InjectionPoints.After;
                }

                injectionCallback(context);
            }
        }

        private static bool CheckFilter(MethodDefinition targetMethod, 
            string targetName, 
            CustomAttribute aspectAttribute)
        {
            var result = true;

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

        private static bool CheckTarget(MethodDefinition targetMethod, InjectionTargets targets)
        {
            if (targetMethod.IsAbstract || targetMethod.IsStatic)
            {
                return false;
            }

            if (targetMethod.IsConstructor)
            {
                return (targets & InjectionTargets.Constructor) != 0;
            }

            if (targetMethod.IsGetter)
            {
                return (targets & InjectionTargets.Getter) != 0;
            }

            if (targetMethod.IsSetter)
            {
                return (targets & InjectionTargets.Setter) != 0;
            }

            if (targetMethod.IsAddOn)
            {
                return (targets & InjectionTargets.EventAdd) != 0;
            }

            if (targetMethod.IsRemoveOn)
            {
                return (targets & InjectionTargets.EventRemove) != 0;
            }

            return (targets & InjectionTargets.Method) != 0;
        }

        private static IEnumerable<MethodDefinition> GetAdviceMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        private static IEnumerable<AdviceArgumentSource> GetAdviceArgumentsSources(MethodDefinition adviceMethod)
        {
            foreach (var argument in adviceMethod.Parameters)
            {
                var argumentAttribute = argument.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
                if (argumentAttribute == null)
                    throw new CompilationException("Unbound advice arguments are not supported", adviceMethod);

                var source = (AdviceArgumentSource)argumentAttribute.ConstructorArguments[0].Value;
                if (source == AdviceArgumentSource.Instance)
                {
                    if (!argument.ParameterType.IsTypeOf(typeof(object)))
                        throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Instance", adviceMethod);
                }
                else if (source == AdviceArgumentSource.TargetArguments)
                {
                    if (!argument.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
                        throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);
                }
                else if (source == AdviceArgumentSource.TargetName)
                {
                    if (!argument.ParameterType.IsTypeOf(typeof(string)))
                        throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);
                }
                else if (source == AdviceArgumentSource.AbortFlag)
                {
                    if (!argument.ParameterType.IsTypeOf(new ByReferenceType(adviceMethod.Module.TypeSystem.Boolean)))
                        throw new CompilationException("Argument should be of type ref System.Boolean to inject AdviceArgumentSource.AbortFlag", adviceMethod);
                }

                yield return source;
            }
        }
    }
}
