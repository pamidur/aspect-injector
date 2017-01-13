using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;
using System.Diagnostics;

namespace AspectInjector.Core
{
    public class Logger
    {
        public virtual bool IsErrorThrown { get; private set; }

        public virtual void LogError(CompilationError error)
        {
            LogError(error.Message);
            Trace.WriteLine($"{error.SequencePoint.Document.Url} line:{error.SequencePoint.StartLine}");
        }

        public virtual void LogError(string message)
        {
            Trace.TraceError(message);
            IsErrorThrown = true;
        }

        public virtual void LogInformation(string message)
        {
            Trace.TraceInformation(message);
        }

        public virtual void LogMessage(string message)
        {
            Trace.WriteLine(message);
        }

        public virtual void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }
    }
}