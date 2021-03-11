using System;

namespace Aspects.Universal.Events
{
    public class AspectExceptionEventArgs : AspectEventArgs
    {
        public Exception Exception { get; set; }
    }
}