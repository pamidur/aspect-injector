using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Common
{
    [Serializable]
    internal class CompilationException : Exception
    {
        public CompilationException(string message, SequencePoint sp)
            : base(message)
        {
            SequencePoint = sp ?? new SequencePoint(new Document(""));
        }

        public CompilationException(string message, MethodReference mr)
            : this(message, mr != null ? mr.Resolve().Body.Instructions.FirstOrDefault(i => i.SequencePoint != null).SequencePoint : null)
        {
        }

        public CompilationException(string message, TypeReference tr)
            : this(message, tr != null ? tr.Resolve().Methods.FirstOrDefault(m => m.Body.Instructions.Any(i => i.SequencePoint != null)) : null)
        {
        }

        public SequencePoint SequencePoint { get; private set; }
    }
}