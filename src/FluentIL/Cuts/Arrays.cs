using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FluentIL.Extensions;
using System;

namespace FluentIL;

public static class Arrays
{
    public static Cut CreateArray(this in Cut cut, ITypeDefOrRef elementType, params PointCut[] elements)
    {
        var pc = cut
            .Write(Instruction.CreateLdcI4(elements.Length))
            .Write(Instruction.Create(OpCodes.Newarr, elementType));

        for (var i = 0; i < elements.Length; i++)
        {
            pc = pc.Write(OpCodes.Dup);
            pc = SetByIndex(pc, elementType, i, elements[i]);
        }

        return pc;
    }

    public static Cut GetByIndex(this in Cut pc, ITypeDefOrRef elementType, int index)
    {
        return pc
            .Write(Instruction.CreateLdcI4(index))
            .Write(GetLoadOpcode(elementType));
    }

    public static Cut SetByIndex(this in Cut pc, ITypeDefOrRef elementType, int index, PointCut value)
    {
        return pc
            .Write(Instruction.CreateLdcI4(index))
            .Here(value)
            .Write(GetStoreOpcode(elementType));
    }

    private static OpCode GetLoadOpcode(ITypeDefOrRef elementType)
    {
        var sig = elementType.ToTypeSig();

        return sig.ElementType switch
        {
            ElementType.Class => OpCodes.Ldelem_Ref,
            ElementType.Object => OpCodes.Ldelem_Ref,
            ElementType.R8 => OpCodes.Ldelem_R8,
            ElementType.R4 => OpCodes.Ldelem_R4,
            ElementType.I8 => OpCodes.Ldelem_I8,
            ElementType.U8 => OpCodes.Ldelem_I8,
            ElementType.I4 => OpCodes.Ldelem_I4,
            ElementType.U4 => OpCodes.Ldelem_U4,
            ElementType.I2 => OpCodes.Ldelem_I2,
            ElementType.U2 => OpCodes.Ldelem_U2,
            ElementType.U1 => OpCodes.Ldelem_U1,
            ElementType.I1 => OpCodes.Ldelem_I1,
            ElementType.Boolean => OpCodes.Ldelem_I1,
            ElementType.String => OpCodes.Ldelem_Ref,
            _ => throw new NotSupportedException($"No instruction for {sig.ElementType}")
        };
    }

    private static OpCode GetStoreOpcode(ITypeDefOrRef elementType)
    {
        var def = elementType.ResolveTypeDef();
        var sig = def.IsEnum ? def.GetEnumUnderlyingType() : elementType.ToTypeSig();

        return sig.ElementType switch
        {
            ElementType.Class => OpCodes.Stelem_Ref,
            ElementType.Object => OpCodes.Stelem_Ref,
            ElementType.R8 => OpCodes.Stelem_R8,
            ElementType.R4 => OpCodes.Stelem_R4,
            ElementType.I8 => OpCodes.Stelem_I8,
            ElementType.U8 => OpCodes.Stelem_I8,
            ElementType.I4 => OpCodes.Stelem_I4,
            ElementType.U4 => OpCodes.Stelem_I4,
            ElementType.I2 => OpCodes.Stelem_I2,
            ElementType.U2 => OpCodes.Stelem_I2,
            ElementType.U1 => OpCodes.Stelem_I1,
            ElementType.I1 => OpCodes.Stelem_I1,
            ElementType.Boolean => OpCodes.Stelem_I1,
            ElementType.String => OpCodes.Stelem_Ref,
            _ => throw new NotSupportedException($"No instruction for {sig.ElementType}"),
        };
    }
}

