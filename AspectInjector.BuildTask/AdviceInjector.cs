using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectInjector.BuildTask
{
    public class AdviceInjector : InjectorBase, IModuleProcessor
    {
        public virtual void ProcessModule(ModuleDefinition module)
        {
            foreach (var @class in module.Types.Where(t => t.IsClass))
            {
                var classAspectAttributes = @class.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();

                foreach (var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter))
                {
                    var methodAspectAttributes = method.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    ProcessMethod(method, method.Name, classAspectAttributes.Union(methodAspectAttributes));
                }

                foreach (var property in @class.Properties)
                {
                    var propertyAspectAttributes = property.CustomAttributes.GetAttributesOfType<AspectAttribute>().ToList();
                    var allAspectAttributes = propertyAspectAttributes.Union(classAspectAttributes).ToList();

                    if (property.GetMethod != null)
                    {
                        ProcessMethod(property.GetMethod, property.Name, allAspectAttributes);
                    }
                    if (property.SetMethod != null)
                    {
                        ProcessMethod(property.SetMethod, property.Name, allAspectAttributes);
                    }
                }
            }
        }

        private void ProcessMethod(MethodDefinition targetMethod,
            string targetName,
            IEnumerable<CustomAttribute> aspectAttributes)
        {
            foreach (var aspectAttribute in aspectAttributes.Where(a => CheckFilter(targetMethod, targetName, a)))
            {
                var aspectType = (TypeDefinition)aspectAttribute.ConstructorArguments.First().Value;
                var aspectInstanceField = GetOrCreateAspectReference(targetMethod.DeclaringType, aspectType);

                foreach (var adviceMethod in GetAdviceMethods(aspectType))
                {
                    ProcessAdvice(adviceMethod, aspectInstanceField, targetMethod, targetName);
                }
            }
        }

        private void ProcessAdvice(MethodDefinition adviceMethod,
            FieldReference aspectInstanceField,
            MethodDefinition targetMethod,
            string targetName)
        {
            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            //todo:: rethink getting attribute parameters
            var targetsObject = adviceAttribute.GetPropertyValue("Targets");
            var targets = (InjectionTargets)(targetsObject ?? InjectionTargets.Constructor | InjectionTargets.Getter | InjectionTargets.Method | InjectionTargets.Setter);

            var pointsObject = adviceAttribute.GetPropertyValue("Points");
            var points = (InjectionPoints)(pointsObject ?? InjectionPoints.After | InjectionPoints.Before);

            if (CheckTarget(targetMethod, targets))
            {
                if ((points & InjectionPoints.Before) != 0)
                {
                    InjectAdvice(aspectInstanceField,
                        adviceMethod,
                        targetMethod,
                        targetName,
                        targetMethod.Body.Instructions.First());
                }
                if ((points & InjectionPoints.After) != 0)
                {
                    InjectAdvice(aspectInstanceField,
                        adviceMethod,
                        targetMethod,
                        targetName,
                        targetMethod.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).First());
                }
            }
        }

        private void InjectAdvice(FieldReference aspectInstanceField,
            MethodDefinition adviceMethod,
            MethodDefinition targetMethod,
            string targetName,
            Instruction lastInstruction)
        {
            ILProcessor processor = targetMethod.Body.GetILProcessor();
            processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Nop));
            processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldarg_0));
            processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldfld, aspectInstanceField));

            foreach (var argument in adviceMethod.Parameters)
            {
                var argumentAttribute = argument.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
                if (argumentAttribute == null)
                {
                    throw new NotSupportedException("Unbound advice arguments are not supported");
                }

                var source = (AdviceArgumentSource)argumentAttribute.GetPropertyValue("Source");
                switch (source)
                {
                    case AdviceArgumentSource.Instance:
                        if (!argument.ParameterType.IsType(typeof(object)))
                        {
                            //todo:: throw appropriate exception
                        }
                        processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldarg_0));
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        throw new NotSupportedException("Bindind of method arguments to advice arguments is not supported yet");

                    case AdviceArgumentSource.TargetName:
                        if (!argument.ParameterType.IsType(typeof(string)))
                        {
                            //todo:: throw appropriate exception
                        }
                        processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldstr, targetName));
                        break;
                }
            }

            processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Callvirt, adviceMethod));
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

        private bool CheckFilter(MethodDefinition targetMethod, string targetName, CustomAttribute aspectAttribute)
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
    }
}