using AspectInjector.Core.Services;
using System;

namespace AspectInjector.CLI
{
    public class ConsoleLogger : Logger
    {
        protected override void Write(string text, MessageType type)
        {
            switch (type)
            {
                case MessageType.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case MessageType.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case MessageType.Info: Console.ForegroundColor = ConsoleColor.Green; break;
            }

            Console.Write(text);

            Console.ResetColor();
        }
    }
}