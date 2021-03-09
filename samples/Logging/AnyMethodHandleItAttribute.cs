using System;
using AspectInjector.Broker;
using Aspects.Universal.Aspects;
using Aspects.Universal.Attributes;
using Aspects.Universal.Events;

namespace Logging
{
    [Injection(typeof(MethodWrapperAspect))]
    internal class AnyMethodHandleItAttribute : MethodAspectAttribute
    {
        public override void OnBefore(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnBefore method {eventArgs.Name}");
        }

        public override void OnAfter(AspectEventArgs eventArgs)
        {
            Console.WriteLine($"OnAfter method {eventArgs.Name}");
        }

        public override void OnException(AspectExceptionEventArgs eventArgs)
        {
            Console.WriteLine($"OnException method {eventArgs.Name} --> {eventArgs.Exception}");
        }
    }
}