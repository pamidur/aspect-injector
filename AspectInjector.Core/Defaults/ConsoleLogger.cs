using AspectInjector.Core.Contracts;
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

        public void LogException(Exception exception)
        {
            LogError(exception.Message);
            Console.WriteLine(exception.StackTrace);
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