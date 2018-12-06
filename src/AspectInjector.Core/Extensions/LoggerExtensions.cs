using AspectInjector.Core.Contracts;
using Microsoft.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void Log(this ILogger log, DiagnosticDescriptor descriptor, params string[] messages)
        {
            log.Log(descriptor, null, messages);
        }

        public static void Log(this ILogger log, DiagnosticDescriptor descriptor, ICustomAttributeProvider ap, params string[] messages)
        {
            if (ap is IMemberDefinition md) Log(log, descriptor, md, messages);
            else log.Log(descriptor, null, messages);
        }

        public static void Log(this ILogger log, DiagnosticDescriptor descriptor, IMemberDefinition md, params string[] messages)
        {
            switch (md)
            {
                case MethodDefinition method: Log(log, descriptor, method, messages); break;
                case TypeDefinition type: Log(log, descriptor, type, messages); break;

                default: log.Log(descriptor, null, messages); break;
            }
        }

        public static void Log(this ILogger log, DiagnosticDescriptor descriptor, MethodDefinition md, params string[] messages)
        {
            log.Log(descriptor, GetSPFromMethod(md), messages);
        }

        public static void Log(this ILogger log, DiagnosticDescriptor descriptor, TypeDefinition td, params string[] messages)
        {
            var sp = td.Methods.Select(GetSPFromMethod).Where(s => s != null).FirstOrDefault();
            log.Log(descriptor, sp, messages);
        }

        private static SequencePoint GetSPFromMethod(MethodDefinition md)
        {
            return md.DebugInformation.SequencePoints.Where(s => s.Document != null).FirstOrDefault();
        }
    }
}
