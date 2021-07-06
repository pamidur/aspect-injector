using System;
using System.Threading.Tasks;
using Aspects.Universal.Events;

namespace Aspects.Universal.Attributes
{
    public abstract class BaseMethodPointsAspectAttribute : BaseUniversalWrapperAttribute
    {
        protected internal sealed override T WrapSync<T>(Func<object[], T> target, object[] args, AspectEventArgs eventArgs)
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

        protected internal sealed override async Task<T> WrapAsync<T>(Func<object[], Task<T>> target, object[] args, AspectEventArgs eventArgs)
        {
            OnBefore(eventArgs);

            try
            {
                var result = await target(args);
                OnAfter(eventArgs);

                return result;
            }
            catch (Exception exception)
            {
                return OnException<T>(eventArgs, exception);
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
    }
}