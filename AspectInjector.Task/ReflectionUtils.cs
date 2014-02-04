using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal static class ReflectionUtils
    {
        public static bool IsType(this TypeReference typeReference, Type type)
        {
            return typeReference.Resolve().FullName == type.FullName;
        }

        public static bool HasType(this IEnumerable<TypeReference> typeReferences, Type type)
        {
            return typeReferences.Any(td => td.IsType(type));
        }

        public static bool IsAttributeOfType(this CustomAttribute attribute, Type attributeType)
        {
            return attribute.AttributeType.Resolve().FullName == attributeType.FullName;
        }

        public static bool HasAttributeOfType(this IEnumerable<CustomAttribute> attributes, Type attributeType)
        {
            return attributes.Any(a => a.IsAttributeOfType(attributeType));
        }

        public static string GetMethodName<T>(Expression<Action<T>> expression)
        {
            var methodExpression = expression.Body as MethodCallExpression;
            return methodExpression != null ? methodExpression.Method.Name : null;
        }

        public static MethodReference ImportMethod<T>(this ModuleDefinition module, TypeDefinition type, Expression<Action<T>> methodExpression)
        {
            var methodName = GetMethodName<T>(methodExpression);
            return !string.IsNullOrEmpty(methodName) ? module.Import(type.Methods.First(c => c.Name == methodName)) : null;
        }

        public static MethodReference ImportConstructor(this ModuleDefinition module, TypeDefinition type)
        {
            return module.Import(type.Methods.First(c => c.Name == ".ctor"));
        }
    }
}
