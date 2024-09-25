using AspectInjector.Broker;
using System;
using System.Diagnostics;

namespace Aspects.Logging
{
    [Injection(typeof(LoggingAspect))]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false)]
    public class LogAttribute : Attribute
    {
        // Uses Stopwatch to measure execution time
        public bool MeasureTime { get; set; } = false;
    }    
}
