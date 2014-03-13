using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Extensions
{
    public static class ILProcessorExtension
    {
        public static Instruction CreateOptimized(this ILProcessor processor, OpCode opCode, int value)
        {
            if (opCode == OpCodes.Ldarg || opCode == OpCodes.Ldarg_S)
                return OptimizeLoadArgument(processor, checked((ushort)value));

            if (opCode == OpCodes.Ldc_I4 || opCode == OpCodes.Ldc_I4_S)
                return OptimizeLoadInt32(processor, value);

            if (opCode == OpCodes.Ldloc || opCode == OpCodes.Ldloc_S)
                return OptimizeLoadLocal(processor, checked((ushort)value));

            if (opCode == OpCodes.Stloc || opCode == OpCodes.Stloc_S)
                return OptimizeStoreLocal(processor, checked((ushort)value));

            return processor.Create(opCode, value);
        }

        private static Instruction OptimizeLoadLocal(ILProcessor processor, ushort localIndex)
        {
            if (localIndex == 0)
                return processor.Create(OpCodes.Ldloc_0);

            if (localIndex == 1)
                return processor.Create(OpCodes.Ldloc_1);

            if (localIndex == 2)
                return processor.Create(OpCodes.Ldloc_2);

            if (localIndex == 3)
                return processor.Create(OpCodes.Ldloc_3);

            if (localIndex <= 255)
                processor.Create(OpCodes.Ldloc_S, localIndex);

            return processor.Create(OpCodes.Ldloc, localIndex);
        }

        private static Instruction OptimizeStoreLocal(ILProcessor processor, ushort localIndex)
        {
            if (localIndex == 0)
                return processor.Create(OpCodes.Stloc_0);

            if (localIndex == 1)
                return processor.Create(OpCodes.Stloc_1);

            if (localIndex == 2)
                return processor.Create(OpCodes.Stloc_2);

            if (localIndex == 3)
                return processor.Create(OpCodes.Stloc_3);

            if (localIndex <= 255)
                processor.Create(OpCodes.Stloc_S, localIndex);

            return processor.Create(OpCodes.Stloc, localIndex);
        }

        private static Instruction OptimizeLoadArgument(ILProcessor processor, ushort argumentIndex)
        {
            if (argumentIndex == 0)
                return processor.Create(OpCodes.Ldarg_0);

            if (argumentIndex == 1)
                return processor.Create(OpCodes.Ldarg_1);

            if (argumentIndex == 2)
                return processor.Create(OpCodes.Ldarg_2);

            if (argumentIndex == 3)
                return processor.Create(OpCodes.Ldarg_3);

            if (argumentIndex <= 255)
                processor.Create(OpCodes.Ldarg_S, argumentIndex);

            return processor.Create(OpCodes.Ldarg, argumentIndex);
        }

        private static Instruction OptimizeLoadInt32(ILProcessor processor, int integer)
        {
            if (integer == 0)
                return processor.Create(OpCodes.Ldc_I4_0);

            if (integer == 1)
                return processor.Create(OpCodes.Ldc_I4_1);

            if (integer == 2)
                return processor.Create(OpCodes.Ldc_I4_2);

            if (integer == 3)
                return processor.Create(OpCodes.Ldc_I4_3);

            if (integer == 4)
                return processor.Create(OpCodes.Ldc_I4_4);

            if (integer == 5)
                return processor.Create(OpCodes.Ldc_I4_5);

            if (integer == 6)
                return processor.Create(OpCodes.Ldc_I4_6);

            if (integer == 7)
                return processor.Create(OpCodes.Ldc_I4_7);

            if (integer == 8)
                return processor.Create(OpCodes.Ldc_I4_8);

            if (integer >= -128 && integer <= 127)
                processor.Create(OpCodes.Ldc_I4_S, integer);

            return processor.Create(OpCodes.Ldc_I4, integer);
        }
    }
}
