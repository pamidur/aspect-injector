using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class CompilationError
    {
        public CompilationError(string message, SequencePoint sp)
        {
            SequencePoint = sp ?? new SequencePoint(new Document(string.Empty));
        }

        public CompilationError(string message, Instruction inst)
            : this(message, inst == null ? null : inst.SequencePoint)
        {
        }

        public CompilationError(string message, MethodReference mr)
            : this(message, mr == null ? null : mr.Resolve().Body.Instructions.FirstOrDefault(i => i.SequencePoint != null))
        {
        }

        public CompilationError(string message, TypeReference tr)
            : this(message, tr == null ? null : tr.Resolve().Methods.FirstOrDefault(m => m.Body.Instructions.Any(i => i.SequencePoint != null)))
        {
        }

        public string Message { get; set; }

        public SequencePoint SequencePoint { get; set; }
    }
}