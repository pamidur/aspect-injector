using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FluentIL.Extensions;
using System;

namespace FluentIL
{
    public static class TypeMembers
    {
        public static Cut ThisOrStatic(this in Cut cut) =>
            cut.Method.HasThis ? cut.This() : cut;

        public static Cut ThisOrNull(this in Cut cut) =>
            cut.Method.HasThis ? cut.This() : cut.Null();

        public static Cut This(this in Cut cut)
        {
            if (cut.Method.HasThis) return cut.Write(OpCodes.Ldarg_0);
            else throw new InvalidOperationException("Attempt to load 'this' on static method.");
        }

        public static Cut Load(this in Cut cut, Local variable) => cut
            .Write(OpCodes.Ldloc, variable);

        public static Cut Load(this in Cut cut, Parameter par) => cut
            .Write(OpCodes.Ldarg, par);

        public static Cut Load(this in Cut cut, IField field)
        {
            if (!field.IsCallCompatible())
                throw new ArgumentException($"Uninitialized generic call reference: {field}");

            var fieldDef = field.ResolveFieldDef();
            return cut.Write(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
        }

        public static Cut LoadRef(this in Cut cut, Local variable) => cut
            .Write(Instruction.Create(OpCodes.Ldloca, variable));

        public static Cut LoadRef(this in Cut cut, Parameter par) => cut
            .Write(Instruction.Create(OpCodes.Ldarga, par));

        public static Cut LoadRef(this in Cut cut, IField field)
        {
            if (!field.IsCallCompatible())
                throw new ArgumentException($"Uninitialized generic call reference: {field}");

            var fieldDef = field.ResolveFieldDef();

            return cut.Write(fieldDef.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, field);
        }

        public static Cut Store(this in Cut cut, IField field, PointCut value = null)
        {
            if (!field.IsCallCompatible())
                throw new ArgumentException($"Uninitialized generic call reference: {field}");

            var fieldDef = field.ResolveFieldDef();

            return cut
                .Here(value)
                .Write(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
        }

        public static Cut Store(this in Cut cut, Local variable, PointCut value = null) => cut
           .Here(value)
           .Write(OpCodes.Stloc, variable);

        public static Cut Store(this in Cut cut, Parameter par, PointCut value = null)
        {
            if (par.ParamDef.IsIn | par.ParamDef.IsOut)
            {
                return cut
                    .Load(par)
                    .Here(value);
            }
            else
            {
                return cut
                    .Here(value)
                    .Write(OpCodes.Starg, par);
            }
        }
    }
}
