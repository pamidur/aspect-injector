using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;
using System.Diagnostics;

namespace AspectInjector.Core.Defaults
{
    public class TraceLogger : ILogger
    {
        public bool IsErrorThrown { get; private set; }

        public void LogError(CompilationError error)
        {
            LogError(error.Message);
            Trace.WriteLine($"{error.SequencePoint.Document.Url} line:{error.SequencePoint.StartLine}");
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
            IsErrorThrown = true;
        }

        public void LogInformation(string message)
        {
            Trace.TraceInformation(message);
        }

        public void LogMessage(string message)
        {
            Trace.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }
    }
}