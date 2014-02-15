using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    public class AdviceInjector : InjectorBase, IAspectInjector
    {
        public virtual void ProcessModule(ModuleDefinition module)
        {
            foreach(var @class in module.Types.Where(t => t.IsClass))
            {
                var classAspectAttributes = @class.CustomAttributes.Where(a => a.IsAttributeOfType(typeof(AspectAttribute))).ToList();

                foreach(var method in @class.Methods.Where(m => !m.IsSetter && !m.IsGetter))
                {
                    var methodAspectAttributes = method.CustomAttributes.Where(a => a.IsAttributeOfType(typeof(AspectAttribute))).ToList();
                    ProcessMethod(method, method.Name, classAspectAttributes.Union(methodAspectAttributes));
                    methodAspectAttributes.ForEach(a => method.CustomAttributes.Remove(a));
                }

                foreach (var property in @class.Properties)
                {
                    var propertyAspectAttributes = property.CustomAttributes.Where(a => a.IsAttributeOfType(typeof(AspectAttribute))).ToList();
                    var allAspectAttributes = propertyAspectAttributes.Union(classAspectAttributes).ToList();

                    if (property.GetMethod != null)
                    {
                        ProcessMethod(property.GetMethod, property.Name, allAspectAttributes);
                    }
                    if (property.SetMethod != null)
                    {
                        ProcessMethod(property.SetMethod, property.Name, allAspectAttributes);
                    }

                    propertyAspectAttributes.ForEach(a => property.CustomAttributes.Remove(a));
                }

                classAspectAttributes.ForEach(a => @class.CustomAttributes.Remove(a));
            }
        }

        private void ProcessMethod(MethodDefinition targetMethod, 
            string targetName,
            IEnumerable<CustomAttribute> aspectAttributes)
        {
            foreach (var aspectAttribute in aspectAttributes)
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
            var adviceAttribute = adviceMethod.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(AdviceAttribute)));

            //todo:: rethink getting attribute parameters
            var targetsObject = adviceAttribute.Properties.Where(p => p.Name == "Targets").Select(p => p.Argument.Value).FirstOrDefault();
            var targets = (InjectionTarget)(targetsObject ?? InjectionTarget.Constructor | InjectionTarget.Getter | InjectionTarget.Method | InjectionTarget.Setter);

            var pointsObject = adviceAttribute.Properties.Where(p => p.Name == "Points").Select(p => p.Argument.Value).FirstOrDefault();
            var points = (InjectionPoint)(pointsObject ?? InjectionPoint.After | InjectionPoint.Before);

            if(CheckTargetMethod(targetMethod, targets))
            {
                if ((points & InjectionPoint.Before) != 0)
                {
                    InjectAdvice(aspectInstanceField, 
                        adviceMethod,
                        targetMethod,
                        targetName,
                        targetMethod.Body.Instructions.First());
                }
                if ((points & InjectionPoint.After) != 0)
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

            foreach(var argument in adviceMethod.Parameters)
            {
                var argumentAttribute = argument.CustomAttributes.FirstOrDefault(a => a.IsAttributeOfType(typeof(AdviceArgumentAttribute)));
                if (argumentAttribute == null)
                {
                    throw new NotSupportedException("Unbound advice arguments are not supported");
                }

                var source = (AdviceArgumentSource)argumentAttribute.Properties.Where(p => p.Name == "Source").Select(p => p.Argument.Value).First();
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
            return aspectType.Methods.Where(m => m.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(AdviceAttribute))));
        }

        private bool CheckTargetMethod(MethodDefinition targetMethod, InjectionTarget targets)
        {
            if (targetMethod.IsAbstract || targetMethod.IsStatic)
            {
                return false;
            }

            if (targetMethod.IsConstructor)
            {
                return (targets & InjectionTarget.Constructor) != 0;
            }

            if (targetMethod.IsGetter)
            {
                return (targets & InjectionTarget.Getter) != 0;
            }

            if (targetMethod.IsSetter)
            {
                return (targets & InjectionTarget.Setter) != 0;
            }

            return (targets & InjectionTarget.Method) != 0;
        }
    }
}