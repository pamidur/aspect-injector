using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class CompilationException : Exception
    {
        public CompilationException(string message, SequencePoint sp)
            : base(message)
        {
            SequencePoint = sp ?? new SequencePoint(new Document(""));
        }

        public CompilationException(string message, MethodReference mr)
            : this(message, mr.Resolve().Body.Instructions.First(i => i.SequencePoint != null).SequencePoint)
        {
        }

        public CompilationException(string message, TypeReference tr)
            : this(message, tr.Resolve().Methods.First(m => m.Body.Instructions.Any(i => i.SequencePoint != null)))
        {
        }

        public SequencePoint SequencePoint { get; private set; }
    }
}