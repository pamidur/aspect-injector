using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class CompilationError
    {
        public CompilationError(string message, SequencePoint sp)
        {
            Message = message;
            SequencePoint = sp ?? new SequencePoint(new Document(string.Empty));
        }

        public string Message { get; set; }
        public SequencePoint SequencePoint { get; set; }

        public static CompilationError From(string message, Instruction inst)
        {
            return new CompilationError(message, inst.SequencePoint);
        }

        public static CompilationError From<T>(string message, T source)
            where T : class, ICustomAttributeProvider
        {
            if (source is TypeDefinition)
            {
                var td = (TypeDefinition)(object)source;
                return From(message, td.Methods.FirstOrDefault(m => m.Body.Instructions.Any(i => i.SequencePoint != null)));
            }

            if (source is MethodDefinition)
            {
                var md = (MethodDefinition)(object)source;
                return From(message, md.Body.Instructions.FirstOrDefault(i => i.SequencePoint != null));
            }

            return new CompilationError(message, null);
        }
    }
}