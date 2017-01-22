using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.CLI
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

        public void LogError(CompilationMessage error)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (error.SequencePoint?.Document != null)
                Console.Write($"{error.SequencePoint.Document.Url}({error.SequencePoint.StartLine},{error.SequencePoint.StartColumn}): ");

            Console.WriteLine($"error: { error.Text}");

            Console.ResetColor();
            IsErrorThrown = true;
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