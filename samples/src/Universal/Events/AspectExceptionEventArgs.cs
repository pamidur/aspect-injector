using System;

namespace Aspects.Universal.Events
{
    public class AspectExceptionEventArgs : AspectEventArgs
    {
        public Exception Exception { get; set; }

        public static AspectExceptionEventArgs CreateFrom(AspectEventArgs args, Exception exception)
        {
            var aspectExceptionEventArgs = (AspectExceptionEventArgs) args.Copy();
            aspectExceptionEventArgs.Exception = exception;

            return aspectExceptionEventArgs;
        }
    }
}