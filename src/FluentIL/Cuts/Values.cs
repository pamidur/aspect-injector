using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FluentIL.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace FluentIL
{
    public static class Values
    {
        public static Cut Pop(this in Cut pc) => pc
            .Write(OpCodes.Pop);

        public static Cut Null(this in Cut pc) => pc
            .Write(OpCodes.Ldnull);

        public static Cut Dup(this in Cut pc) => pc
            .Write(OpCodes.Dup);

        public static Cut Delegate(this in Cut cut, IMethod method) => cut
            .Write(OpCodes.Ldftn, method);

        public static Cut TypeOf(this in Cut cut, ITypeDefOrRef type) => cut
            .Write(OpCodes.Ldtoken, type)
            .Write(OpCodes.Call, GetTypeFromHandleMethod_Ref(cut));

        public static Cut MethodOf(this in Cut cut, IMethod method) => cut
            .Write(OpCodes.Ldtoken, method)
            .Write(OpCodes.Ldtoken, method.DeclaringType)
            .Write(OpCodes.Call, GetMethodFromHandleMethod_Ref(cut));

        public static Cut Value(this in Cut pc, object value)
        {
            if (value == null)
                return Null(pc);

            var valueType = value.GetType();

            if (value is CAArgument argument)
                return AttributeArgument(pc, argument);
            else if (value is TypeRef tr)
                return TypeOf(pc, tr);
            else if (valueType.IsValueType)
                return Primitive(pc, value, value.GetType());
            else if (value is string str)
                return pc.Write(OpCodes.Ldstr, str);
            else
                throw new NotSupportedException(valueType.ToString());
        }

        public static Cut Primitive<T>(this in Cut pc, T value) where T : struct
        {
            return pc.Primitive(value, typeof(T));
        }

        private static Cut Primitive(this in Cut pc, object value, Type valueType)
        {
            return value switch
            {
                bool bo => pc.Write(bo ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0),
                long l => pc.Write(Instruction.Create(OpCodes.Ldc_I8, l)),
                ulong ul => pc.Write(Instruction.Create(OpCodes.Ldc_I8, unchecked((long)ul))),
                double d => pc.Write(Instruction.Create(OpCodes.Ldc_R8, d)),
                int i => pc.Write(Instruction.CreateLdcI4(i)),
                uint ui => pc.Write(Instruction.CreateLdcI4(unchecked((int)ui))),
                float fl => pc.Write(Instruction.Create(OpCodes.Ldc_R4, fl)),
                sbyte sb => pc.Write(Instruction.CreateLdcI4(sb)),
                byte b => pc.Write(Instruction.CreateLdcI4(b)),
                ushort us => pc.Write(Instruction.CreateLdcI4(us)),
                short s => pc.Write(Instruction.CreateLdcI4(s)),
                char c => pc.Write(Instruction.CreateLdcI4(c)),
                _ => throw new NotSupportedException(valueType.ToString()),
            };
        }

        public static Cut Cast(this in Cut cut, TypeSig typeOnStack, TypeSig expectedType)
        {
            var result = cut;

            if (typeOnStack.Match(expectedType))
                return result;

            if (expectedType.IsByRef)
            {
                var elementType = expectedType.Next;
                result = result.Cast(typeOnStack, elementType);
                return StoreByReference(result, elementType);
            }

            if (typeOnStack.IsByRef)
            {
                typeOnStack = typeOnStack.Next;
                result = LoadByReference(result, typeOnStack);
            }

            if (typeOnStack.Match(expectedType))
                return result;

            if (expectedType.IsValueType || expectedType.IsGenericParameter)
            {
                if (!typeOnStack.IsValueType)
                    return result.Write(OpCodes.Unbox_Any, expectedType);
            }
            else
            {
                if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
                    return result.Write(OpCodes.Box, typeOnStack);
                else if (!expectedType.Match(cut.TypeSystem.Object))
                    return result.Write(OpCodes.Castclass, expectedType);
                else
                    return result;
            }

            throw new InvalidCastException($"Cannot cast '{typeOnStack}' to '{expectedType}'");
        }

        private static Cut StoreByReference(in Cut pc, TypeSig sig)
        {
            return sig.ElementType switch
            {
                ElementType.Class => pc.Write(OpCodes.Stind_Ref),
                ElementType.Object => pc.Write(OpCodes.Stind_Ref),
                ElementType.String => pc.Write(OpCodes.Stind_Ref),
                ElementType.Array => pc.Write(OpCodes.Stind_Ref),
                ElementType.MVar => pc.Write(OpCodes.Stobj, sig),
                ElementType.Var => pc.Write(OpCodes.Stobj, sig),
                ElementType.ValueType => pc.Write(OpCodes.Stobj, sig),
                ElementType.GenericInst => pc.Write(OpCodes.Stobj, sig),
                ElementType.R8 => pc.Write(OpCodes.Stind_R8),
                ElementType.R4 => pc.Write(OpCodes.Stind_R4),
                ElementType.I8 => pc.Write(OpCodes.Stind_I8),
                ElementType.U8 => pc.Write(OpCodes.Stind_I8),
                ElementType.I4 => pc.Write(OpCodes.Stind_I4),
                ElementType.U4 => pc.Write(OpCodes.Stind_I4),
                ElementType.I2 => pc.Write(OpCodes.Stind_I2),
                ElementType.U2 => pc.Write(OpCodes.Stind_I2),
                ElementType.U1 => pc.Write(OpCodes.Stind_I1),
                ElementType.I1 => pc.Write(OpCodes.Stind_I1),
                ElementType.Boolean => pc.Write(OpCodes.Stind_I1),
                ElementType.Char => pc.Write(OpCodes.Stind_I2),
                ElementType.Ptr => pc.Write(OpCodes.Stind_I),
                _ => throw new NotSupportedException($"No instruction for {sig.ElementType}"),
            };
        }

        private static Cut LoadByReference(in Cut pc, TypeSig sig)
        {
            return sig.ElementType switch
            {
                ElementType.Class => pc.Write(OpCodes.Ldind_Ref),
                ElementType.Object => pc.Write(OpCodes.Ldind_Ref),
                ElementType.String => pc.Write(OpCodes.Ldind_Ref),
                ElementType.Array => pc.Write(OpCodes.Ldind_Ref),
                ElementType.MVar => pc.Write(OpCodes.Ldobj, sig),
                ElementType.Var => pc.Write(OpCodes.Ldobj, sig),
                ElementType.ValueType => pc.Write(OpCodes.Ldobj, sig),
                ElementType.GenericInst => pc.Write(OpCodes.Ldobj, sig),
                ElementType.R8 => pc.Write(OpCodes.Ldind_R8),
                ElementType.R4 => pc.Write(OpCodes.Ldind_R4),
                ElementType.I8 => pc.Write(OpCodes.Ldind_I8),
                ElementType.U8 => pc.Write(OpCodes.Ldind_I8),
                ElementType.I4 => pc.Write(OpCodes.Ldind_I4),
                ElementType.U4 => pc.Write(OpCodes.Ldind_U4),
                ElementType.I2 => pc.Write(OpCodes.Ldind_I2),
                ElementType.U2 => pc.Write(OpCodes.Ldind_U2),
                ElementType.U1 => pc.Write(OpCodes.Ldind_U1),
                ElementType.I1 => pc.Write(OpCodes.Ldind_I1),
                ElementType.Boolean => pc.Write(OpCodes.Ldind_U1),
                ElementType.Char => pc.Write(OpCodes.Ldind_U2),
                ElementType.Ptr => pc.Write(OpCodes.Ldind_I),
                _ => throw new NotSupportedException($"No instruction for {sig.ElementType}"),
            };
        }

        private static Cut AttributeArgument(in Cut cut, CAArgument argument)
        {
            var val = argument.Value;

            var pc = cut;

            if (val != null && val.GetType().IsArray)
                pc = pc.CreateArray(
                    argument.Type.Next.ToTypeDefOrRef(),
                    ((Array)val).Cast<object>().Select<object, PointCut>(v => (in Cut il) => il.Value(v)).ToArray()
                    );
            else
            {
                pc = pc.Value(val);

                if (val is CAArgument next)
                    pc = pc.Cast(next.Type, argument.Type);
            }

            return pc;
        }

        private static MemberRef GetTypeFromHandleMethod_Ref(in Cut cut)
        {
            var tref = cut.Import(StandardType.Type);
            var mr = new MemberRefUser(cut.Method.Module, "GetTypeFromHandle", new MethodSig(CallingConvention.StdCall, 0,
                tref.ToTypeSig(),
                cut.Import(StandardType.RuntimeTypeHandle).ToTypeSig()
                ));

            return mr;
        }

        private static MemberRef GetMethodFromHandleMethod_Ref(in Cut cut)
        {
            var mbref = cut.Import(StandardType.MethodBase);
            var mr = new MemberRefUser(cut.Method.Module, "GetMethodFromHandle", new MethodSig(CallingConvention.StdCall, 0,
                mbref.ToTypeSig(),
                cut.Import(StandardType.RuntimeMethodHandle).ToTypeSig(),
                cut.Import(StandardType.RuntimeTypeHandle).ToTypeSig()
                ));

            return mr;
        }
    }
}
