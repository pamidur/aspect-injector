using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;
using System.Diagnostics;

namespace AspectInjector.Core.Services
{
    public class Logger : ILogger
    {
        protected enum MessageType
        {
            Info,
            Warning,
            Error
        }

        public virtual bool IsErrorThrown { get; private set; }

        public virtual void LogInfo(string message)
        {
            WriteLine($"Info: {message}", MessageType.Info);
        }

        public virtual void LogWarning(CompilationMessage message)
        {
            if (message.SequencePoint?.Document != null)
                Write($"{message.SequencePoint.Document.Url}({message.SequencePoint.StartLine},{message.SequencePoint.StartColumn},{message.SequencePoint.EndLine},{message.SequencePoint.EndColumn}): ", MessageType.Warning);

            WriteLine($"Warning: { message.Text}", MessageType.Warning);
        }

        public virtual void LogError(CompilationMessage message)
        {
            if (message.SequencePoint?.Document != null)
                Write($"{message.SequencePoint.Document.Url}({message.SequencePoint.StartLine},{message.SequencePoint.StartColumn},{message.SequencePoint.EndLine},{message.SequencePoint.EndColumn}): ", MessageType.Error);

            WriteLine($"Error: { message.Text}", MessageType.Error);

            IsErrorThrown = true;
        }

        protected virtual void WriteLine(string text, MessageType type)
        {
            Write(text, type);
            Write(Environment.NewLine, type);
        }

        protected virtual void Write(string text, MessageType type)
        {
            Trace.Write(text);
        }
    }
}