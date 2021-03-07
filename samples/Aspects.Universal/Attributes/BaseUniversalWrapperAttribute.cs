using System;
using Aspects.Universal.Events;

namespace Aspects.Universal.Attributes
{
    public abstract class BaseUniversalWrapperAttribute : Attribute
    {
        public virtual void OnBefore(AspectEventArgs eventArgs)
        {
        }

        public virtual void OnAfter(AspectEventArgs eventArgs)
        {
        }

        public virtual void OnException(AspectExceptionEventArgs eventArgs)
        {
        }
    }
}