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
            SequencePoint = sp ?? new SequencePoint(Instruction.Create(OpCodes.Nop), new Document(string.Empty));
        }

        public string Text { get; set; }

        public SequencePoint SequencePoint { get; set; }

        public static CompilationMessage From(string text, MethodDefinition scope, Instruction inst)
        {


            while (inst != null && scope.DebugInformation.GetSequencePoint(inst) == null && inst.Previous != null)
                inst = inst.Previous;

            return new CompilationMessage(text, inst == null ? null : scope.DebugInformation.GetSequencePoint(inst));
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
                return From(text, td.Methods.FirstOrDefault(m => m.DebugInformation.GetSequencePointMapping().Any()));
            }

            if (source is MethodDefinition)
            {
                var md = (MethodDefinition)(object)source;
                return new CompilationMessage(text, md.DebugInformation.GetSequencePointMapping().FirstOrDefault().Value);
            }

            //if (source is ParameterDefinition)
            //{
            //    var pd = (ParameterDefinition)(object)source;
            //    return From(text, pd..Body.Instructions.FirstOrDefault(i => i.SequencePoint != null));
            //}

            return new CompilationMessage(text, null);
        }
    }
}