using System;

namespace Aspects.Universal.Events
{
    public class AspectExceptionEventArgs : AspectEventArgs
    {
        public Exception Exception { get; set; }

        public static AspectExceptionEventArgs CreateFrom(AspectEventArgs aspectEventArgs, Exception exception)
        {
            return new AspectExceptionEventArgs
            {
                Args = aspectEventArgs.Args,
                Exception = exception,
                Instance = aspectEventArgs.Instance,
                Method = aspectEventArgs.Method,
                Name = aspectEventArgs.Name,
                ReturnType = aspectEventArgs.ReturnType,
                Target = aspectEventArgs.Target,
                Triggers = aspectEventArgs.Triggers,
                Type = aspectEventArgs.Type
            };
        }
    }
}