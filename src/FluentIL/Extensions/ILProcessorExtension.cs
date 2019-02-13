using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class ILProcessorExtension
    {
        public static void SafeRemove(this ILProcessor processor, Instruction target)
        {
            var next = target.Next;
            var prev = target.Previous;

            processor.Redirect(target, next, prev);
            processor.Remove(target);
        }

        public static Instruction SafeReplace(this ILProcessor processor, Instruction target, Instruction instruction)
        {
            processor.Redirect(target, instruction, instruction);
            processor.Replace(target, instruction);
            return instruction;
        }

        public static void Redirect(this ILProcessor processor, Instruction source, Instruction to, Instruction from)
        {
            var refs = processor.Body.Instructions.Where(i => i.Operand == source).ToList();

            if (refs.Any())
            {
                if (to == null)
                    throw new InvalidOperationException();

                foreach (var rref in refs)
                    rref.Operand = to;
            }


            foreach (var handler in processor.Body.ExceptionHandlers)
            {
                if (handler.FilterStart == source)
                    handler.FilterStart = to?? throw new InvalidOperationException();

                if (handler.HandlerEnd == source)
                    handler.HandlerEnd = from ?? throw new InvalidOperationException();

                if (handler.HandlerStart == source)
                    handler.HandlerStart = to ?? throw new InvalidOperationException();

                if (handler.TryEnd == source)
                    handler.TryEnd = from ?? throw new InvalidOperationException();

                if (handler.TryStart == source)
                    handler.TryStart = to ?? throw new InvalidOperationException();
            }
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