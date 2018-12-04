using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace AspectInjector.Core.Fluent
{
    public static class Arrays
    {
        public static PointCut CreateArray(this PointCut pc, TypeReference elementType, params Action<PointCut>[] elements)
        {
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldc_I4, elements.Length));
            pc = pc.Append(pc.CreateInstruction(OpCodes.Newarr, elementType));

            for (var i = 0; i < elements.Length; i++)
            {
                pc = pc.Append(pc.CreateInstruction(OpCodes.Dup));
                SetByIndex(pc, elementType, i, elements[i]);
            }

            return pc;
        }

        public static PointCut GetByIndex(this PointCut pc, int index)
        {
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldc_I4, index));
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldelem_Ref));
            return pc;
        }

        public static PointCut GetAddrByIndex(this PointCut pc, int index, TypeReference type)
        {
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldc_I4, index));
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldelema, type));
            return pc;
        }

        public static PointCut SetByIndex(this PointCut pc, TypeReference elementType, int index, Action<PointCut> value)
        {
            pc = pc.Append(pc.CreateInstruction(OpCodes.Ldc_I4, index));
            value(pc);
            pc = pc.Append(pc.CreateInstruction(GetStoreOpcode(elementType)));
            return pc;
        }

        private static OpCode GetStoreOpcode(TypeReference elementType)
        {
            switch(elementType.MetadataType)
            {
                case MetadataType.Class: return OpCodes.Stelem_Ref;
                case MetadataType.Object: return OpCodes.Stelem_Ref;
                case MetadataType.Double: return OpCodes.Stelem_R8;
                case MetadataType.Single: return OpCodes.Stelem_R4;
                case MetadataType.Int64: return OpCodes.Stelem_I8;
                case MetadataType.UInt64: return OpCodes.Stelem_I8;
                case MetadataType.Int32: return OpCodes.Stelem_I4;
                case MetadataType.UInt32: return OpCodes.Stelem_I4;
                case MetadataType.Int16: return OpCodes.Stelem_I2;
                case MetadataType.UInt16: return OpCodes.Stelem_I2;
                case MetadataType.Byte: return OpCodes.Stelem_I1;
                case MetadataType.SByte: return OpCodes.Stelem_I1;
                case MetadataType.Boolean: return OpCodes.Stelem_I1;
                //case MetadataType.GenericInstance: return OpCodes.Stelem_Any;
            }

            throw new NotSupportedException();
        }
    }
}
