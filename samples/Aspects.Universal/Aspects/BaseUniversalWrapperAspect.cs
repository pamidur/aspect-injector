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
                Target = target,
                Name = name,
                Args = args,
                ReturnType = returnType,
                Triggers = triggers.OfType<BaseUniversalWrapperAttribute>().ToArray()
            };

            if (typeof(Task).IsAssignableFrom(eventArgs.ReturnType))
            {
                var syncResultType = eventArgs.ReturnType.IsConstructedGenericType ? eventArgs.ReturnType.GenericTypeArguments[0] : _voidTaskResult;

                return _asyncHandler.MakeGenericMethod(syncResultType).Invoke(this, new object[] { eventArgs.Copy() });
            }

            var syncReturnType = eventArgs.ReturnType == typeof(void) ? typeof(object) : eventArgs.ReturnType;
            return _syncHandler.MakeGenericMethod(syncReturnType).Invoke(this, new object[] { eventArgs.Copy() });
        }

        private T WrapSync<T>(AspectEventArgs eventArgs)
        {
            OnBefore(eventArgs);

            try
            {
                var result = (T)eventArgs.Target(eventArgs.Args);

                OnAfter(eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                OnException(AspectExceptionEventArgs.CreateFrom(eventArgs, exception));

                return default;
            }
        }

        private async Task<T> WrapAsync<T>(AspectEventArgs eventArgs)
        {
            OnBefore(eventArgs);

            try
            {
                var result = await (Task<T>)eventArgs.Target(eventArgs.Args);

                OnAfter(eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                OnException(AspectExceptionEventArgs.CreateFrom(eventArgs, exception));

                return default;
            }
        }

        private void OnBefore(AspectEventArgs eventArgs)
        {
            foreach (var attr in eventArgs.Triggers)
            {
                try
                {
                    attr.OnBefore(eventArgs);
                }
                catch
                {
                }
            }
        }

        private void OnAfter(AspectEventArgs eventArgs)
        {
            foreach (var attr in eventArgs.Triggers)
            {
                try
                {
                    attr.OnAfter(eventArgs);
                }
                catch
                {
                }
            }
        }

        private void OnException(AspectExceptionEventArgs eventArgs)
        {
            foreach (var attr in eventArgs.Triggers)
            {
                try
                {
                    attr.OnException(eventArgs);
                }
                catch
                {
                }
            }
        }
    }
}