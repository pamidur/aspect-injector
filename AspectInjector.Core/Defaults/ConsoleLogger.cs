using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;

namespace AspectInjector.Core.Defaults
{
    public class ConsoleLogger : ILogger
    {
        public bool IsErrorThrown { get; private set; }

        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            IsErrorThrown = true;
        }

        public void LogError(CompilationError error)
        {
            LogError(error.Message);
            Console.WriteLine($"{error.SequencePoint.Document.Url} line:{error.SequencePoint.StartLine}");
        }

        public void LogInformation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}