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
        private class ArgumentSourceInfo
        {
            public ParameterDefinition Argument { get; set; }
            public AdviceArgumentSource Source { get; set; }
        }

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
            var checkedAttributes = aspectAttributes.Where(a => CheckFilter(targetMethod, a)).ToList();

            if (checkedAttributes.Count > 0)
            {
                ILProcessor processor = targetMethod.Body.GetILProcessor();

                var originRet = targetMethod.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
                var newRet = processor.Create(OpCodes.Ret);
                var newPreRet = processor.Create(OpCodes.Nop);

                processor.InsertAfter(originRet, newRet);
                processor.Replace(originRet, newPreRet);


                foreach (var aspectAttribute in checkedAttributes)
                {
                    var aspectType = (TypeDefinition)aspectAttribute.ConstructorArguments.First().Value;
                    var aspectInstanceField = GetOrCreateAspectReference(targetMethod.DeclaringType, aspectType);

                    foreach (var adviceMethod in GetAdviceMethods(aspectType))
                    {
                        ProcessAdvice(adviceMethod, aspectInstanceField, targetMethod, newRet, newPreRet);
                    }
                }
            }
        }

        private void ProcessAdvice(MethodDefinition adviceMethod,
            FieldReference aspectInstanceField,
            MethodDefinition targetMethod,
            Instruction returnPoint,
            Instruction preReturnPont
            )
        {
            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            var pointsObject = adviceAttribute.ConstructorArguments[0].Value;
            var points = (InjectionPoints)(pointsObject ?? InjectionPoints.After | InjectionPoints.Before);

            var targetsObject = adviceAttribute.ConstructorArguments[1].Value;
            var targets = (InjectionTargets)(targetsObject ?? InjectionTargets.Constructor | InjectionTargets.Getter | InjectionTargets.Method | InjectionTargets.Setter);

            if (CheckTarget(targetMethod, targets))
            {
                ILProcessor processor = targetMethod.Body.GetILProcessor();

                if ((points & InjectionPoints.Before) != 0)
                {
                    var args = ExtactArguments(adviceMethod);
                    if (args.Any(a => a.Source == AdviceArgumentSource.AbortFlag))
                    {
                        if (!targetMethod.ReturnType.IsTypeOf(adviceMethod.ReturnType))
                            throw new CompilationException("Return types of advice (" + adviceMethod.FullName + ") and target (" + targetMethod.FullName + ") should be the same.", targetMethod);

                        InjectAdviceWithResultReplacement(aspectInstanceField,
                        adviceMethod,
                        targetMethod,
                        targetMethod.Body.Instructions.First(), preReturnPont);
                    }
                    else
                    {
                        if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                            throw new CompilationException("Advice of InjectionPoints.Before and without argument of AdviceArgumentSource.AbortFlag can be System.Void only.", adviceMethod);

                        var callArgs = GetCallArguments(args, targetMethod, null);
                        InjectMethodCall(processor, targetMethod.Body.Instructions.First(), aspectInstanceField, adviceMethod, callArgs.ToArray());
                    }
                }
                if ((points & InjectionPoints.After) != 0)
                {
                    if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionPoints.After can be System.Void only.", adviceMethod);

                    var args = ExtactArguments(adviceMethod);
                    if (args.Any(a => a.Source == AdviceArgumentSource.AbortFlag))
                        throw new CompilationException("Method should have a return value and inject into InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag.", adviceMethod);

                    var callArgs = GetCallArguments(args, targetMethod, null);
                    InjectMethodCall(processor, returnPoint, aspectInstanceField, adviceMethod, callArgs.ToArray());
                }
            }
        }


        private static readonly string _abortMethodVarName = "__a$_do_abort_method";
        private static readonly string _abortMethodResultVarName = "__a$_abort_method_result";

        private void InjectAdviceWithResultReplacement(FieldReference aspectInstanceField,
            MethodDefinition adviceMethod,
            MethodDefinition targetMethod,
            Instruction injectionPoint,
            Instruction returnPoint
            )
        {
            ILProcessor processor = targetMethod.Body.GetILProcessor();

            var abortMethodVar = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == _abortMethodVarName);
            if (abortMethodVar == null)
            {
                abortMethodVar = processor.CreateLocalVariable(
                       injectionPoint,
                       targetMethod.Module.TypeSystem.Boolean,
                       false, _abortMethodVarName);
            }
            else
            {
                processor.SetLocalVariable(abortMethodVar, injectionPoint, false);
            }

            VariableDefinition methodResultVar = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == _abortMethodResultVarName);
            if (methodResultVar == null && !adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                methodResultVar = processor.CreateLocalVariable<object>(
                    injectionPoint,
                    targetMethod.Module.Import(adviceMethod.ReturnType),
                    null, _abortMethodResultVarName);
            }

            var args = ExtactArguments(adviceMethod);
            var callArgs = GetCallArguments(args, targetMethod, abortMethodVar);

            InjectMethodCall(processor, injectionPoint, aspectInstanceField, adviceMethod, callArgs.ToArray());

            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Stloc, methodResultVar.Index));
            }

            processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, abortMethodVar.Index));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_1));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ceq));

            var continuePoint = processor.Create(OpCodes.Nop);
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Brfalse_S, continuePoint));

            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, methodResultVar.Index));
            }
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Br_S, returnPoint));

            processor.InsertBefore(injectionPoint, continuePoint);
        }

        private List<ArgumentSourceInfo> ExtactArguments(MethodDefinition adviceMethod)
        {
            var arg = new List<ArgumentSourceInfo>();

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
                if (source == AdviceArgumentSource.TargetArguments)
                {
                    if (!argument.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
                        throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);
                }
                if (source == AdviceArgumentSource.TargetName)
                {
                    if (!argument.ParameterType.IsTypeOf(typeof(string)))
                        throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);
                }
                if (source == AdviceArgumentSource.AbortFlag)
                {
                    if (!argument.ParameterType.IsTypeOf(new ByReferenceType(adviceMethod.Module.TypeSystem.Boolean)))
                        throw new CompilationException("Argument should be of type ref System.Boolean to inject AdviceArgumentSource.AbortTarget", adviceMethod);
                }

                arg.Add(new ArgumentSourceInfo { Argument = argument, Source = source });
            }

            return arg;
        }

        private List<object> GetCallArguments(IEnumerable<ArgumentSourceInfo> argumetSourceInfoes, MethodDefinition targetMethod, VariableDefinition abortMethodVar)
        {
            var args = new List<object>();

            foreach (var argumentInfo in argumetSourceInfoes)
            {
                switch (argumentInfo.Source)
                {
                    case AdviceArgumentSource.Instance:
                        args.Add(Markers.InstanceSelfMarker);
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        args.Add(targetMethod.Parameters.ToArray());
                        break;

                    case AdviceArgumentSource.TargetName:
                        args.Add(targetMethod.Name);
                        break;

                    case AdviceArgumentSource.AbortFlag:
                        args.Add(abortMethodVar);
                        break;
                }
            }
            return args;
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