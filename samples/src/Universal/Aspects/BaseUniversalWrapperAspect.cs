using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aspects.Universal.Attributes;
using Aspects.Universal.Events;

namespace Aspects.Universal.Aspects
{
    public abstract class BaseUniversalWrapperAspect
    {
        private static readonly MethodInfo _asyncHandler = typeof(BaseUniversalWrapperAspect).GetMethod(nameof(WrapAsync), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _syncHandler = typeof(BaseUniversalWrapperAspect).GetMethod(nameof(WrapSync), BindingFlags.NonPublic | BindingFlags.Instance);
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
            };

            var baseUniversalWrapperAttributes = triggers.OfType<BaseUniversalWrapperAttribute>().ToArray();

            if (typeof(Task).IsAssignableFrom(eventArgs.ReturnType))
            {
                var syncResultType = eventArgs.ReturnType.IsConstructedGenericType ? eventArgs.ReturnType.GenericTypeArguments[0] : _voidTaskResult;

                return _asyncHandler.MakeGenericMethod(syncResultType).Invoke(this, new object[] { target, args, baseUniversalWrapperAttributes, eventArgs });
            }

            var syncReturnType = eventArgs.ReturnType == typeof(void) ? typeof(object) : eventArgs.ReturnType;
            return _syncHandler.MakeGenericMethod(syncReturnType).Invoke(this, new object[] { target, args, baseUniversalWrapperAttributes, eventArgs });
        }

        private T WrapSync<T>(Func<object[], object> target, object[] args, BaseUniversalWrapperAttribute[] attributes, AspectEventArgs eventArgs)
        {
            OnBefore(attributes, eventArgs);

            try
            {
                var result = (T)target(args);

                OnAfter(attributes, eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                OnException(attributes, CreateAspectExceptionEventArgs(eventArgs, exception));

                return default;
            }
        }

        private async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, BaseUniversalWrapperAttribute[] attributes, AspectEventArgs eventArgs)
        {
            OnBefore(attributes, eventArgs);

            try
            {
                var result = await (Task<T>)target(args);

                OnAfter(attributes, eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                OnException(attributes, CreateAspectExceptionEventArgs(eventArgs, exception));

                return default;
            }
        }

        private void OnBefore(BaseUniversalWrapperAttribute[] attributes, AspectEventArgs eventArgs)
        {
            foreach (var attribute in attributes)
            {
                attribute.OnBefore(eventArgs);
            }
        }

        private void OnAfter(BaseUniversalWrapperAttribute[] attributes, AspectEventArgs eventArgs)
        {
            foreach (var attribute in attributes)
            {
                attribute.OnAfter(eventArgs);
            }
        }

        private void OnException(BaseUniversalWrapperAttribute[] attributes, AspectExceptionEventArgs eventArgs)
        {
            foreach (var attribute in attributes)
            {
                attribute.OnException(eventArgs);
            }
        }

        private static AspectExceptionEventArgs CreateAspectExceptionEventArgs(AspectEventArgs aspectEventArgs, Exception exception)
        {
            return new AspectExceptionEventArgs
            {
                Args = aspectEventArgs.Args,
                Exception = exception,
                Instance = aspectEventArgs.Instance,
                Method = aspectEventArgs.Method,
                Name = aspectEventArgs.Name,
                ReturnType = aspectEventArgs.ReturnType,
                Type = aspectEventArgs.Type
            };
        }
    }
}