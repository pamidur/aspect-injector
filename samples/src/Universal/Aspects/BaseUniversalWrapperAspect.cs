using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Aspects.Universal.Attributes;
using Aspects.Universal.Events;

namespace Aspects.Universal.Aspects
{
    public abstract class BaseUniversalWrapperAspect
    {
        private delegate object Method(object[] args);
        private delegate object Wrapper(Func<object[], object> target, object[] args);
        private delegate object Handler(Func<object[], object> next, object[] args, AspectEventArgs eventArgs);

        private static readonly ConcurrentDictionary<(MethodBase, Type), Lazy<Handler>> _delegateCache = new ConcurrentDictionary<(MethodBase, Type), Lazy<Handler>>();

        private static readonly MethodInfo _asyncGenericHandler =
            typeof(BaseUniversalWrapperAttribute).GetMethod(nameof(BaseUniversalWrapperAttribute.WrapAsync), BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo _syncGenericHandler =
            typeof(BaseUniversalWrapperAttribute).GetMethod(nameof(BaseUniversalWrapperAttribute.WrapSync), BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Type _voidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult");

        protected object BaseHandle(
            object instance,
            Type type,
            MethodBase method,
            Func<object[], object> target,
            string name,
            object[] args,
            Type returnType,
            Attribute[] triggers)
        {
            var eventArgs = new AspectEventArgs
            {
                Instance = instance,
                Type = type,
                Method = method,
                Name = name,
                Args = args,
                ReturnType = returnType,
                Triggers = triggers
            };

            var wrappers = triggers.OfType<BaseUniversalWrapperAttribute>().ToArray();

            var handler = GetMethodHandler(method, returnType, wrappers);
            return handler(target, args, eventArgs);
        }

        private Handler CreateMethodHandler(Type returnType, IReadOnlyList<BaseUniversalWrapperAttribute> wrappers)
        {
            var targetParam = Expression.Parameter(typeof(Func<object[], object>), "orig");
            var eventArgsParam = Expression.Parameter(typeof(AspectEventArgs), "event");

            MethodInfo wrapperMethod;

            if (typeof(Task).IsAssignableFrom(returnType))
            {
                var taskType = returnType.IsConstructedGenericType ? returnType.GenericTypeArguments[0] : _voidTaskResult;
                returnType = typeof(Task<>).MakeGenericType(new[] { taskType });

                wrapperMethod = _asyncGenericHandler.MakeGenericMethod(new[] { taskType });
            }
            else
            {
                if (returnType == typeof(void))
                    returnType = typeof(object);

                wrapperMethod = _syncGenericHandler.MakeGenericMethod(new[] { returnType });
            }

            var converArgs = Expression.Parameter(typeof(object[]), "args");
            var next = Expression.Lambda(Expression.Convert(Expression.Invoke(targetParam, converArgs), returnType), converArgs);

            foreach (var wrapper in wrappers)
            {
                var argsParam = Expression.Parameter(typeof(object[]), "args");
                next = Expression.Lambda(Expression.Call(Expression.Constant(wrapper), wrapperMethod, next, argsParam, eventArgsParam), argsParam);
            }

            var orig_args = Expression.Parameter(typeof(object[]), "orig_args");
            var handler = Expression.Lambda<Handler>(Expression.Convert(Expression.Invoke(next, orig_args), typeof(object)), targetParam, orig_args, eventArgsParam);

            var handlerCompiled = handler.Compile();

            return handlerCompiled;
        }

        private Handler GetMethodHandler(MethodBase method, Type returnType, IReadOnlyList<BaseUniversalWrapperAttribute> wrappers)
        {
            var lazyHandler = _delegateCache.GetOrAdd((method, returnType), _ => new Lazy<Handler>(() => CreateMethodHandler(returnType, wrappers)));
            return lazyHandler.Value;
        }
    }
}
