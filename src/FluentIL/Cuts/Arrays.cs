using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace FluentIL
{
    public static class Arrays
    {
        public static Cut CreateArray(this Cut pc, TypeReference elementType, params PointCut[] elements)
        {
            pc = pc
                .Write(OpCodes.Ldc_I4, elements.Length)
                .Write(OpCodes.Newarr, elementType);

            for (var i = 0; i < elements.Length; i++)
            {
                pc = pc.Write(OpCodes.Dup);
                pc = SetByIndex(pc, elementType, i, elements[i]);
            }

            return pc;
        }

        public static Cut GetByIndex(this Cut pc, TypeReference elementType, int index)
        {
            return pc
                .Write(OpCodes.Ldc_I4, index)
                .Write(GetLoadOpcode(elementType));
        }

        public static Cut SetByIndex(this Cut pc, TypeReference elementType, int index, PointCut value)
        {
            return pc
                .Write(OpCodes.Ldc_I4, index)
                .Here(value)
                .Write(GetStoreOpcode(elementType));
        }

        private static OpCode GetLoadOpcode(TypeReference elementType)
        {
            switch (elementType.MetadataType)
            {
                case MetadataType.Class: return OpCodes.Ldelem_Ref;
                case MetadataType.Object: return OpCodes.Ldelem_Ref;
                case MetadataType.Double: return OpCodes.Ldelem_R8;
                case MetadataType.Single: return OpCodes.Ldelem_R4;
                case MetadataType.Int64: return OpCodes.Ldelem_I8;
                case MetadataType.UInt64: return OpCodes.Ldelem_I8;
                case MetadataType.Int32: return OpCodes.Ldelem_I4;
                case MetadataType.UInt32: return OpCodes.Ldelem_U4;
                case MetadataType.Int16: return OpCodes.Ldelem_I2;
                case MetadataType.UInt16: return OpCodes.Ldelem_U2;
                case MetadataType.Byte: return OpCodes.Ldelem_U1;
                case MetadataType.SByte: return OpCodes.Ldelem_I1;
                case MetadataType.Boolean: return OpCodes.Ldelem_I1;
                    //case MetadataType.GenericInstance: return OpCodes.Stelem_Any;
            }

            throw new NotSupportedException($"No instruction for {elementType.MetadataType.ToString()}");
        }


        private static OpCode GetStoreOpcode(TypeReference elementType)
        {
            switch (elementType.MetadataType)
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

            throw new NotSupportedException($"No instruction for {elementType.MetadataType.ToString()}");
        }
    }
}
