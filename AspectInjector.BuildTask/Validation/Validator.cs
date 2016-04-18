using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectInjector.BuildTask.Validation
{
    //TODO: Fire all compilation exceptions at once

    public static class Validator
    {
        private static List<ValidationRule> _rules = new List<ValidationRule>();

        static Validator()
        {
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Instance, ShouldBeOfType = typeof(object) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Target, ShouldBeOfType = typeof(Func<object[], object>) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Type, ShouldBeOfType = typeof(Type) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.TargetReturnType, ShouldBeOfType = typeof(Type) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Method, ShouldBeOfType = typeof(MethodBase) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Arguments, ShouldBeOfType = typeof(object[]) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.RoutableData, ShouldBeOfType = typeof(Attribute[]) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.Name, ShouldBeOfType = typeof(string) });
            _rules.Add(new ValidationRule() { Source = AdviceArgumentSource.ReturnValue, ShouldBeOfType = typeof(object) });
        }

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

            var rule = _rules.FirstOrDefault(r => r.Source == source);
            if (rule != null)
            {
                if (!parameter.ParameterType.IsTypeOf(rule.ShouldBeOfType))
                    throw new CompilationException("Argument should be of type " + rule.ShouldBeOfType.Namespace + "." + rule.ShouldBeOfType.Name + " to inject AdviceArgumentSource." + source.ToString(), adviceMethod);
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
                if ((target & InjectionTargets.Constructor) == InjectionTargets.Constructor)
                    throw new CompilationException("Advice of InjectionPoints." + context.InjectionPoint.ToString() + " can't be applied to constructors", context.AdviceMethod);

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