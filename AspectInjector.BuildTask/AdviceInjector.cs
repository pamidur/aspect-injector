using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
                        targetMethod.Body.Instructions.First());
                }
                if ((points & InjectionPoints.After) != 0)
                {
                    foreach (var retPoint in targetMethod.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
                    {
                        ILProcessor processor = targetMethod.Body.GetILProcessor();

                        var injectionPoint = processor.Create(OpCodes.Ret);

                        processor.InsertAfter(retPoint, injectionPoint);
                        processor.Replace(retPoint, processor.Create(OpCodes.Nop));

                        InjectAdvice(aspectInstanceField,
                            adviceMethod,
                            targetMethod,
                            injectionPoint);
                    }
                }
            }
        }

        private void InjectAdvice(FieldReference aspectInstanceField,
            MethodDefinition adviceMethod,
            MethodDefinition targetMethod,
            Instruction lastInstruction)
        {
            ILProcessor processor = targetMethod.Body.GetILProcessor();

            var args = new List<object>();

            foreach (var argument in adviceMethod.Parameters)
            {
                var argumentAttribute = argument.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
                if (argumentAttribute == null)
                    throw new CompilationException("Unbound advice arguments are not supported", adviceMethod);

                var source = (AdviceArgumentSource)argumentAttribute.GetPropertyValue("Source");
                switch (source)
                {
                    case AdviceArgumentSource.Instance:
                        if (!argument.ParameterType.IsType(typeof(object)))
                            throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Instance", adviceMethod);

                        args.Add(Markers.InstanceSelfMarker);
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        if (!argument.ParameterType.IsTypeReferenceOf(new ArrayType(adviceMethod.Module.TypeSystem.Object.Resolve())))
                            throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);

                        args.Add(targetMethod.Parameters.ToArray());
                        break;

                    case AdviceArgumentSource.TargetName:
                        if (!argument.ParameterType.IsType(typeof(string)))
                            throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);

                        args.Add(targetMethod.Name);
                        break;
                }
            }

            InjectMethodCall(processor, lastInstruction, aspectInstanceField, adviceMethod, args.ToArray());
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