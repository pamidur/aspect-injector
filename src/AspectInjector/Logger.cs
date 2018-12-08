using AspectInjector.Core.Contracts;
using AspectInjector.Rules;
using Microsoft.CodeAnalysis;
using Mono.Cecil.Cil;
using System;

namespace AspectInjector
{
    public class Logger : ILogger
    {
        private static readonly string _toolName = "AspectInjector";

        public virtual bool IsErrorThrown { get; private set; }

        public void Log(Rule rule, SequencePoint sp, params string[] messages)
        {
            var location = sp?.Document == null ? _toolName :
                $"{sp.Document.Url}({sp.StartLine},{sp.StartColumn},{sp.EndLine},{sp.EndColumn})";

            var message = string.Format(rule.Message.ToString(), messages ?? new string[] { });

            switch (rule.Severity)
            {
                case RuleSeverity.Error: WriteError(rule.Id, location, message); IsErrorThrown = true; break;
                case RuleSeverity.Warning: WriteWarning(rule.Id, location, message); break;
                case RuleSeverity.Info: WriteInfo(rule.Id, location, message); break;
            }
        }

        private void WriteInfo(string id, string location, string message)
        {
            Console.WriteLine($"{location}: {message}");
        }

        private void WriteWarning(string id, string location, string message)
        {
            Console.WriteLine($"{location}: warning {id}: {message}");
        }

        private void WriteError(string id, string location, string message)
        {
            Console.WriteLine($"{location}: error {id}: {message}");
        }
    }
}