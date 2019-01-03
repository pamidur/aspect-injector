using AspectInjector.Core.Contracts;
using AspectInjector.Rules;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void Log(this ILogger log, Rule rule, params string[] messages)
        {
            log.Log(rule, null, messages);
        }

        public static void Log(this ILogger log, Rule rule, ICustomAttributeProvider ap, params string[] messages)
        {
            if (ap is IMemberDefinition md) Log(log, rule, md, messages);
            else log.Log(rule, null, messages);
        }

        public static void Log(this ILogger log, Rule rule, IMemberDefinition md, params string[] messages)
        {
            switch (md)
            {
                case MethodDefinition method: Log(log, rule, method, messages); break;
                case TypeDefinition type: Log(log, rule, type, messages); break;

                default: log.Log(rule, null, messages); break;
            }
        }

        public static void Log(this ILogger log, Rule rule, MethodDefinition md, params string[] messages)
        {
            log.Log(rule, GetSPFromMethod(md), messages);
        }

        public static void Log(this ILogger log, Rule rule, TypeDefinition td, params string[] messages)
        {
            var sp = td.Methods.Select(GetSPFromMethod).Where(s => s != null).FirstOrDefault();
            log.Log(rule, sp, messages);
        }

        private static SequencePoint GetSPFromMethod(MethodDefinition md)
        {
            return md.DebugInformation.SequencePoints.Where(s => s.Document != null).FirstOrDefault();
        }
    }
}
