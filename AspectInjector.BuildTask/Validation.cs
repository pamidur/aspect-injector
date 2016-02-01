using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectInjector.BuildTask
{
    //TODO: Fire all compilation exceptions at once

    public static class Validation
    {
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
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            if (source == AdviceArgumentSource.Target)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(Func<object[], object>)))
                    throw new CompilationException("Argument should be of type Func<object[],object> to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.Type)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(Type)))
                    throw new CompilationException("Argument should be of type System.Type to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.TargetReturnType)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(Type)))
                    throw new CompilationException("Argument should be of type System.Type to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.Method)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(MethodInfo)))
                    throw new CompilationException("Argument should be of type System.Reflection.MethodInfo to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.Arguments)
            {
                if (!parameter.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
                    throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.Name)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(string)))
                    throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.ReturnValue)
            {
                if (!parameter.ParameterType.IsTypeOf(typeof(object)))
                    throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
            else if (source == AdviceArgumentSource.RoutableData)
            {
                if (!parameter.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
                    throw new CompilationException("Argument should be of type System.Object[] to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
            }
        }

        internal static void ValidateAdviceMethod(MethodDefinition adviceMethod)
        {
            if (adviceMethod.GenericParameters.Any() || adviceMethod.ReturnType.IsGenericParameter)
                throw new CompilationException("Advice cannot be generic", adviceMethod);
        }

        internal static void ValidateAdviceInjectionContext(Contexts.AdviceInjectionContext context, InjectionTargets target)
        {
            //if (target == InjectionTargets.Constructor)
            //{
            //    if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
            //        throw new CompilationException("Advice of InjectionTargets.Constructor can be System.Void only", context.AdviceMethod);
            //}

            if (context.InjectionPoint == InjectionPoints.After || context.InjectionPoint == InjectionPoints.Before)
            {
                if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(void)))
                    throw new CompilationException("Advice of InjectionPoints." + context.InjectionPoint.ToString() + " can be System.Void only", context.AdviceMethod);
            }

            if (context.InjectionPoint == InjectionPoints.Around)
            {
                if (!context.AdviceMethod.ReturnType.IsTypeOf(typeof(object)))
                    throw new CompilationException("Advice of InjectionPoints." + context.InjectionPoint.ToString() + " should return System.Object", context.AdviceMethod);
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