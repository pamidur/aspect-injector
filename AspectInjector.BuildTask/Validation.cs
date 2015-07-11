using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    //TODO: Fire all compilation exceptions at once

    public static class Validation
    {
        private static List<TargetMethodContext> _abortableInjectionHistory = new List<TargetMethodContext>();

        public static void ValidateCustomAspectDefinition(CustomAttribute attribute)
        {
            attribute.AttributeType.Resolve().CustomAttributes.GetAttributeOfType<AttributeUsageAttribute>();
        }

        public static void ValidateAdviceMethodParameter(ParameterDefinition parameter, MethodReference adviceMethod)
        {
            var argumentAttribute = parameter.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
            if (argumentAttribute == null)
                throw new CompilationException("Unbound advice arguments are not supported", adviceMethod);

            var source = (AdviceArgumentSource)argumentAttribute.ConstructorArguments[0].Value;
            if (source == AdviceArgumentSource.Instance)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(object)))
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Instance", adviceMethod);
            }
            else if (source == AdviceArgumentSource.TargetArguments)
            {
                if (!parameter.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
                    throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);
            }
            else if (source == AdviceArgumentSource.TargetName)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(string)))
                    throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);
            }
            else if (source == AdviceArgumentSource.AbortFlag)
            {
                if (!parameter.ParameterType.IsTypeOf(new ByReferenceType(adviceMethod.Module.TypeSystem.Boolean)))
                    throw new CompilationException("Argument should be of type ref System.Boolean to inject AdviceArgumentSource.AbortFlag", adviceMethod);
            }
            else if (source == AdviceArgumentSource.TargetValue)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(object)))
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.ReturningValue", adviceMethod);
            }
            else if (source == AdviceArgumentSource.TargetException)
            {
                if (!parameter.ParameterType.IsTypeOf(adviceMethod.Module.TypeSystem.ResolveType(typeof(Exception))))
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Exception", adviceMethod);
            }
            else if (source == AdviceArgumentSource.RoutableData)
            {
                if (!parameter.ParameterType.IsTypeOf(adviceMethod.Module.TypeSystem.Object))
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.RoutableData", adviceMethod);
            }
        }

        internal static void ValidateAdviceMethod(MethodDefinition adviceMethod)
        {
            if (adviceMethod.GenericParameters.Any() || adviceMethod.ReturnType.IsGenericParameter)
                throw new CompilationException("Advice cannot be generic", adviceMethod);
        }

        internal static void ValidateAdviceInjectionContext(Contexts.AdviceInjectionContext context, InjectionTargets target)
        {
            if (context.IsAbortable)
            {
                if (_abortableInjectionHistory.Contains(context.AspectContext.TargetMethodContext))
                    throw new CompilationException("Method may have only one advice with argument of AdviceArgumentSource.AbortFlag applied to it", context.AspectContext.TargetMethodContext.TargetMethod);

                _abortableInjectionHistory.Add(context.AspectContext.TargetMethodContext);
            }

            if (target == InjectionTargets.Constructor)
            {
                if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
                    throw new CompilationException("Advice of InjectionTargets.Constructor can be System.Void only", context.AdviceMethod);

                if (context.IsAbortable)
                    throw new CompilationException("Constructors methods don't support AdviceArgumentSource.AbortFlag", context.AdviceMethod);
            }

            if (context.InjectionPoint == InjectionPoints.Before)
            {
                if (context.IsAbortable && !context.AdviceMethod.ReturnType.IsTypeOf(context.AdviceMethod.ReturnType))
                    throw new CompilationException("Return types of advice (" + context.AdviceMethod.FullName + ") and target (" + context.AspectContext.TargetMethodContext.TargetMethod.FullName + ") should be the same", context.AspectContext.TargetMethodContext.TargetMethod);

                if (!context.IsAbortable && !context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
                    throw new CompilationException("Advice of InjectionPoints.Before without argument of AdviceArgumentSource.AbortFlag can be System.Void only", context.AdviceMethod);
            }

            if (context.InjectionPoint == InjectionPoints.After)
            {
                if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
                    throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", context.AdviceMethod);

                if (context.IsAbortable)
                    throw new CompilationException("Method should inject into only InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", context.AdviceMethod);
            }

            if (context.InjectionPoint == InjectionPoints.Exception)
            {
                if (context.IsAbortable)
                    throw new CompilationException("Method should inject into only InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", context.AdviceMethod);

                if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
                    throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", context.AdviceMethod);
            }
        }

        internal static void ValidateAdviceClassType(TypeDefinition adviceClassType)
        {
            if (adviceClassType.GenericParameters.Any())
                throw new CompilationException("Advice class cannot be generic", adviceClassType);
        }

        internal static void ValidateAspectContexts(IEnumerable<AspectContext> contexts)
        {
            foreach (var context in contexts)
            {
                var aspectFactories = context.AdviceClassType.Methods.Where(m => m.IsStatic && !m.IsConstructor && m.CustomAttributes.HasAttributeOfType<AspectFactoryAttribute>()).ToList();

                if (aspectFactories.Count > 1)
                    throw new CompilationException("Only one method can be AspectFactory", aspectFactories.Last());

                if (context.AdviceClassFactory == null)
                    throw new CompilationException("Cannot find either empty constructor or factory for aspect.", context.AdviceClassType);
            }
        }
    }
}
