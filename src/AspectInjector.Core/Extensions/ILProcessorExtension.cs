using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class ILProcessorExtension
    {
        public static Instruction SafeReplace(this ILProcessor processor, Instruction target, Instruction instruction)
        {
            var refs = processor.Body.Instructions.Where(i => i.Operand == target).ToList();

            foreach (var rref in refs)
            {
                rref.Operand = instruction;
            }

            foreach (var handler in processor.Body.ExceptionHandlers)
            {
                if (handler.FilterStart == target)
                    handler.FilterStart = instruction;

                if (handler.HandlerEnd == target)
                    handler.HandlerEnd = instruction;

                if (handler.HandlerStart == target)
                    handler.HandlerStart = instruction;

                if (handler.TryEnd == target)
                    handler.TryEnd = instruction;

                if (handler.TryStart == target)
                    handler.TryStart = instruction;
            }

            processor.Replace(target, instruction);

            return instruction;
        }

        public static Instruction SafeAppend(this ILProcessor processor, Instruction instruction)
        {
            processor.Append(instruction);

            foreach (var handler in processor.Body.ExceptionHandlers.Where(h => h.HandlerEnd == null).ToList())
                handler.HandlerEnd = instruction;

            return instruction;
        }

        public static Instruction SafeInsertBefore(this ILProcessor processor, Instruction target, Instruction instruction)
        {
            processor.InsertBefore(target, instruction);
            return instruction;
        }

        public static Instruction SafeInsertAfter(this ILProcessor processor, Instruction target, Instruction instruction)
        {
            if (target == processor.Body.Instructions.Last())
                return processor.SafeAppend(instruction);

            processor.InsertAfter(target, instruction);
            return instruction;
        }
    }
}