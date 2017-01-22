using AspectInjector.Core.Models;
using System;
using System.Diagnostics;

namespace AspectInjector.Core
{
    public class Logger
    {
        public virtual bool IsErrorThrown { get; private set; }

        public virtual void LogInformation(string message)
        {
            Trace.TraceInformation(message);
        }

        public virtual void LogMessage(string message)
        {
            WriteLine(message);
        }

        public virtual void LogWarning(CompilationMessage message)
        {
            if (message.SequencePoint?.Document != null)
                Write($"{message.SequencePoint.Document.Url}({message.SequencePoint.StartLine},{message.SequencePoint.StartColumn}): ");

            WriteLine($"Error: { message.Text}");
        }

        public virtual void LogError(CompilationMessage message)
        {
            if (message.SequencePoint?.Document != null)
                Write($"{message.SequencePoint.Document.Url}({message.SequencePoint.StartLine},{message.SequencePoint.StartColumn}): ");

            WriteLine($"Warning: { message.Text}");

            IsErrorThrown = true;
        }

        protected virtual void WriteLine(string text)
        {
            Write(text);
            Write(Environment.NewLine);
        }

        protected virtual void Write(string text)
        {
            Trace.Write(text);
        }
    }
}