using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class CompilationMessage
    {
        public CompilationMessage(string text, SequencePoint sp)
        {
            Text = text;
            SequencePoint = sp ?? new SequencePoint(new Document(string.Empty));
        }

        public string Text { get; set; }

        public SequencePoint SequencePoint { get; set; }

        public static CompilationMessage From(string text, Instruction inst)
        {
            return new CompilationMessage(text, inst.SequencePoint);
        }

        public static CompilationMessage From(string text)
        {
            return new CompilationMessage(text, null);
        }

        public static CompilationMessage From<T>(string text, T source)
            where T : class, ICustomAttributeProvider
        {
            if (source is TypeDefinition)
            {
                var td = (TypeDefinition)(object)source;
                return From(text, td.Methods.FirstOrDefault(m => m.Body.Instructions.Any(i => i.SequencePoint != null)));
            }

            if (source is MethodDefinition)
            {
                var md = (MethodDefinition)(object)source;
                return From(text, md.Body.Instructions.FirstOrDefault(i => i.SequencePoint != null));
            }

            return new CompilationMessage(text, null);
        }
    }
}