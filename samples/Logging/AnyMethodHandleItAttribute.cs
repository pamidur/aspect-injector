using System;
using Aspects.Universal.Attributes;
using Aspects.Universal.Events;

namespace Logging
{
    internal class AnyMethodHandleItAttribute : MethodAspectAttribute
    {
        protected override void OnBefore(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnBefore method {eventArgs.Name}");
        }

        protected override void OnAfter(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnAfter method {eventArgs.Name}");
        }

        protected override T OnException<T>(AspectEventArgs eventArgs, Exception exception)
        {
            Console.WriteLine($"OnException method {eventArgs.Name} --> {exception}");
            throw exception;
        }
    }

    internal class AnyMethodHandleIt2Attribute : MethodAspectAttribute
    {
        protected override void OnBefore(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnBefore2 method {eventArgs.Name}");
        }

        protected override void OnAfter(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnAfter2 method {eventArgs.Name}");
        }

        protected override T OnException<T>(AspectEventArgs eventArgs, Exception exception)
        {
            Console.WriteLine($"OnException2 method {eventArgs.Name} --> {exception}");
            throw exception;
        }
    }
}