using AspectInjector.Contracts;
using System;

namespace AspectInjector.Defaults
{
    public class DefaultLogger : ILogger
    {
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void LogException(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception.Message);
            Console.ResetColor();
            Console.WriteLine(exception.StackTrace);
        }

        public void LogMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}