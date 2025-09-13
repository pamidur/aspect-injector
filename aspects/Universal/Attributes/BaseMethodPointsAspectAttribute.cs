using System;
using System.Threading.Tasks;
using Aspects.Universal.Events;

namespace Aspects.Universal.Attributes
{
    public abstract class BaseMethodPointsAspectAttribute : BaseUniversalWrapperAttribute
    {
        protected internal override T WrapSync<T>(Func<object[], T> target, object[] args, AspectEventArgs eventArgs)
        {
            OnBefore(eventArgs);

            try
            {
                var result = base.WrapSync(target, args, eventArgs);
                OnAfter(eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                return OnException<T>(eventArgs, exception);
            }
        }

        protected internal override async Task<T> WrapAsync<T>(Func<object[], Task<T>> target, object[] args, AspectEventArgs eventArgs)
        {
            await OnBeforeAsync(eventArgs);

            try
            {
                var result = await target(args);
                await OnAfterAsync(eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                return await OnExceptionAsync<T>(eventArgs, exception);
            }
        }

        protected virtual void OnBefore(AspectEventArgs eventArgs)
        {
        }

        protected virtual void OnAfter(AspectEventArgs eventArgs)
        {
        }

        protected virtual T OnException<T>(AspectEventArgs eventArgs, Exception exception)
        {
            throw exception;
        }

        protected virtual Task OnBeforeAsync(AspectEventArgs eventArgs)
        {
            OnBefore(eventArgs);
            return Task.CompletedTask;
        }

        protected virtual Task OnAfterAsync(AspectEventArgs eventArgs)
        {
            OnAfter(eventArgs);
            return Task.CompletedTask;
        }

        protected virtual Task<T> OnExceptionAsync<T>(AspectEventArgs eventArgs, Exception exception)
        {
            return Task.FromResult(OnException<T>(eventArgs, exception));
        }
    }
}