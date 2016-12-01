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
            SequencePoint = sp ?? new SequencePoint(new Document(string.Empty));
        }

        public string Message { get; set; }

        public SequencePoint SequencePoint { get; set; }

        public static CompilationError From(string message, Instruction inst)
        {
            return new CompilationError(message, inst.SequencePoint);
        }

        public static CompilationError From(string message, MethodReference mr)
        {
            return From(message, mr == null ? null : mr.Resolve().Body.Instructions.FirstOrDefault(i => i.SequencePoint != null));
        }

        public static CompilationError From(string message, TypeReference tr)
        {
            return From(message, tr == null ? null : tr.Resolve().Methods.FirstOrDefault(m => m.Body.Instructions.Any(i => i.SequencePoint != null)));
        }

        public static CompilationError From<T>(string message, T source)
            where T : class, ICustomAttributeProvider
        {
            if (source is TypeDefinition)
                return From(message, (TypeDefinition)(object)source);

            if (source is MethodDefinition)
                return From(message, (MethodDefinition)(object)source);

            throw new NotImplementedException();
        }
    }
}