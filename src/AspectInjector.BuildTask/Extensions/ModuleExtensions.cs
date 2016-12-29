using System;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Extensions
{
    internal static class ModuleExtensions
    {
        public static string GetMethodName<T>(Expression<Action<T>> expression)
        {
            var methodExpression = expression.Body as MethodCallExpression;
            return methodExpression != null ? methodExpression.Method.Name : null;
        }

        public static MethodReference ImportConstructor(this ModuleDefinition module, TypeDefinition type)
        {
            return module.Import(type.Methods.First(c => c.IsConstructor));
        }

        public static MethodReference ImportMethod<T>(this ModuleDefinition module, TypeDefinition type, Expression<Action<T>> methodExpression)
        {
            var methodName = GetMethodName<T>(methodExpression);
            return !string.IsNullOrEmpty(methodName) ? module.Import(type.Methods.First(c => c.Name == methodName)) : null;
        }
    }
}
