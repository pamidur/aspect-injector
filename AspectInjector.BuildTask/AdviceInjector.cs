using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectInjector.BuildTask
{
    internal class AdviceInjector : InjectorBase, IModuleProcessor
    {
        public virtual void ProcessModule(ModuleDefinition module)
        {
            foreach (var @class in module.Types.Where(t => t.IsClass))
            {
                var classAspectAttributes = @class.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();

                foreach (var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter && !m.IsAddOn && !m.IsRemoveOn))
                {
                    var methodAspectAttributes = method.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    ProcessMethod(method, classAspectAttributes.Union(methodAspectAttributes));
                }

                foreach (var property in @class.Properties)
                {
                    var propertyAspectAttributes = property.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = propertyAspectAttributes.Union(classAspectAttributes).ToList();

                    if (property.GetMethod != null)
                    {
                        ProcessMethod(property.GetMethod, allAspectAttributes);
                    }
                    if (property.SetMethod != null)
                    {
                        ProcessMethod(property.SetMethod, allAspectAttributes);
                    }
                }
            }
        }

        private void ProcessMethod(MethodDefinition targetMethod,
            IEnumerable<CustomAttribute> aspectAttributes)
        {
            foreach (var aspectAttribute in aspectAttributes.Where(a => CheckFilter(targetMethod, a)))
            {
                var aspectType = (TypeDefinition)aspectAttribute.ConstructorArguments.First().Value;
                var aspectInstanceField = GetOrCreateAspectReference(targetMethod.DeclaringType, aspectType);

                foreach (var adviceMethod in GetAdviceMethods(aspectType))
                {
                    ProcessAdvice(adviceMethod, aspectInstanceField, targetMethod);
                }
            }
        }

        private void ProcessAdvice(MethodDefinition adviceMethod,
            FieldReference aspectInstanceField,
            MethodDefinition targetMethod)
        {
            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            var pointsObject = adviceAttribute.ConstructorArguments[0].Value;
            var points = (InjectionPoints)(pointsObject ?? InjectionPoints.After | InjectionPoints.Before);

            var targetsObject = adviceAttribute.ConstructorArguments[1].Value;
            var targets = (InjectionTargets)(targetsObject ?? InjectionTargets.Constructor | InjectionTargets.Getter | InjectionTargets.Method | InjectionTargets.Setter);
            
            if (CheckTarget(targetMethod, targets))
            {
                ILProcessor processor = targetMethod.Body.GetILProcessor();

                var originRet = targetMethod.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
                var newRet = processor.Create(OpCodes.Ret);
                var newPreRet = processor.Create(OpCodes.Nop);

                processor.InsertAfter(originRet, newRet);
                processor.Replace(originRet, newPreRet);

                if ((points & InjectionPoints.Before) != 0)
                {
                    InjectAdvice(aspectInstanceField,
                        adviceMethod,
                        targetMethod,
                        targetMethod.Body.Instructions.First(), newPreRet);
                }
                if ((points & InjectionPoints.After) != 0)
                {
                    InjectAdvice(aspectInstanceField,
                        adviceMethod,
                        targetMethod,
                        newRet, newRet);
                }
            }
        }

        private void InjectAdvice(FieldReference aspectInstanceField,
            MethodDefinition adviceMethod,
            MethodDefinition targetMethod,
            Instruction injectionPoint,
            Instruction returnPoint)
        {
            ILProcessor processor = targetMethod.Body.GetILProcessor();
            AbortConditionInjector abortConditionInjector = null;

            if (adviceMethod.Parameters
                .SelectMany(a => a.CustomAttributes)
                .Any(a => a.IsAttributeOfType<AdviceArgumentAttribute>() 
                    && (AdviceArgumentSource)a.ConstructorArguments[0].Value == AdviceArgumentSource.AbortFlag))
            {
                abortConditionInjector = new AbortConditionInjector(processor,
                    adviceMethod,
                    targetMethod,
                    injectionPoint,
                    returnPoint);

                abortConditionInjector.InjectVariables();
            }
            
            var adviceCallArguments = new List<object>();

            foreach (var argument in adviceMethod.Parameters)
            {
                var argumentAttribute = argument.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
                if (argumentAttribute == null)
                    throw new CompilationException("Unbound advice arguments are not supported", adviceMethod);

                var source = (AdviceArgumentSource)argumentAttribute.ConstructorArguments[0].Value;
                switch (source)
                {
                    case AdviceArgumentSource.Instance:
                        if (!argument.ParameterType.IsTypeOf(typeof(object)))
                            throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Instance", adviceMethod);

                        adviceCallArguments.Add(Markers.InstanceSelfMarker);
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        if (!argument.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object.Resolve())))
                            throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);

                        adviceCallArguments.Add(targetMethod.Parameters.ToArray());
                        break;

                    case AdviceArgumentSource.TargetName:
                        if (!argument.ParameterType.IsTypeOf(typeof(string)))
                            throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);

                        adviceCallArguments.Add(targetMethod.Name);
                        break;

                    case AdviceArgumentSource.AbortFlag:
                        if (!argument.ParameterType.IsTypeOf(new ByReferenceType(adviceMethod.Module.TypeSystem.Boolean)))
                            throw new CompilationException("Argument should be of type ref System.Boolean to inject AdviceArgumentSource.AbortTarget", adviceMethod);

                        if (!targetMethod.ReturnType.IsTypeOf(adviceMethod.ReturnType))
                            throw new CompilationException("Return types of advice (" + adviceMethod.FullName + ") and target (" + targetMethod.FullName + ") should be the same", targetMethod);

                        adviceCallArguments.Add(abortConditionInjector.AbortFlagVariable);
                        break;
                }
            }

            InjectMethodCall(processor, injectionPoint, aspectInstanceField, adviceMethod, adviceCallArguments.ToArray());

            if (abortConditionInjector != null)
            {
                abortConditionInjector.InjectStatements();
            }
        }

        private IEnumerable<MethodDefinition> GetAdviceMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        private bool CheckTarget(MethodDefinition targetMethod, InjectionTargets targets)
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

        private bool CheckFilter(MethodDefinition targetMethod, CustomAttribute aspectAttribute)
        {
            string targetName = targetMethod.Name;

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
    }
}