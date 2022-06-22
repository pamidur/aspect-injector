using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

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

        public static Cut Delegate(this in Cut cut, MethodReference method) => cut
            .Write(OpCodes.Ldftn, method);

        public static Cut TypeOf(this in Cut cut, TypeReference type) => cut
            .Write(OpCodes.Ldtoken, type)
            .Write(OpCodes.Call, GetTypeFromHandleMethod_Ref(cut));

        public static Cut MethodOf(this in Cut cut, MethodReference method) => cut
            .Write(OpCodes.Ldtoken, method)
            .Write(OpCodes.Ldtoken, method.DeclaringType)
            .Write(OpCodes.Call, GetMethodFromHandleMethod_Ref(cut));

        public static Cut Value(this in Cut pc, object value)
        {
            if (value == null)
                return Null(pc);

            var valueType = value.GetType();

            if (value is CustomAttributeArgument argument)
                return AttributeArgument(pc, argument);
            else if (value is TypeReference tr)
                return TypeOf(pc, tr);
            else if (valueType.IsValueType)
                return Primitive(pc, value);
            else if (value is string str)
                return pc.Write(OpCodes.Ldstr, str);
            else
                throw new NotSupportedException(valueType.ToString());
        }

        public static Cut Primitive(this in Cut pc, object value)
        {
            var valueType = value.GetType();

            switch (value)
            {
#pragma warning disable S2583 // Conditionally executed code should be reachable
                case bool bo: return pc.Write(bo ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
#pragma warning restore S2583 // Conditionally executed code should be reachable
                case long l: return pc.Write(OpCodes.Ldc_I8, l);
                case ulong ul: return pc.Write(OpCodes.Ldc_I8, unchecked((long)ul));
                case double d: return pc.Write(OpCodes.Ldc_R8, d);
                case int i: return pc.Write(OpCodes.Ldc_I4, i);
                case uint ui: return pc.Write(OpCodes.Ldc_I4, unchecked((int)ui));
                case float fl: return pc.Write(OpCodes.Ldc_R4, fl);
                case sbyte sb: return pc.Write(OpCodes.Ldc_I4, (int)sb);
                case byte b: return pc.Write(OpCodes.Ldc_I4, (int)b);
                case ushort us: return pc.Write(OpCodes.Ldc_I4, us);
                case short s: return pc.Write(OpCodes.Ldc_I4, s);
                case char c: return pc.Write(OpCodes.Ldc_I4, c);

                default: throw new NotSupportedException(valueType.ToString());
            }
        }

        public static Cut Cast(this in Cut cut, TypeReference typeOnStack, TypeReference expectedType)
        {
            var result = cut;

            if (typeOnStack.Match(expectedType))
                return result;

            if (expectedType.IsByReference)
            {
                var elementType = ((ByReferenceType)expectedType).ElementType;
                result = result.Cast(typeOnStack, elementType);
                return StoreByReference(result, elementType);
            }

            if (typeOnStack.IsByReference)
            {
                typeOnStack = ((ByReferenceType)typeOnStack).ElementType;
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

        private static Cut StoreByReference(in Cut pc, TypeReference elemType)
        {
            switch (elemType.MetadataType)
            {
                case MetadataType.Class: return pc.Write(OpCodes.Stind_Ref);
                case MetadataType.Object: return pc.Write(OpCodes.Stind_Ref);
                case MetadataType.String: return pc.Write(OpCodes.Stind_Ref);
                case MetadataType.Array: return pc.Write(OpCodes.Stind_Ref);
                case MetadataType.MVar: return pc.Write(OpCodes.Stobj, elemType);
                case MetadataType.Var: return pc.Write(OpCodes.Stobj, elemType);
                case MetadataType.ValueType: return pc.Write(OpCodes.Stobj, elemType);
                case MetadataType.GenericInstance: return pc.Write(OpCodes.Stobj, elemType);
                case MetadataType.Double: return pc.Write(OpCodes.Stind_R8);
                case MetadataType.Single: return pc.Write(OpCodes.Stind_R4);
                case MetadataType.Int64: return pc.Write(OpCodes.Stind_I8);
                case MetadataType.UInt64: return pc.Write(OpCodes.Stind_I8);
                case MetadataType.Int32: return pc.Write(OpCodes.Stind_I4);
                case MetadataType.UInt32: return pc.Write(OpCodes.Stind_I4);
                case MetadataType.Int16: return pc.Write(OpCodes.Stind_I2);
                case MetadataType.UInt16: return pc.Write(OpCodes.Stind_I2);
                case MetadataType.Byte: return pc.Write(OpCodes.Stind_I1);
                case MetadataType.SByte: return pc.Write(OpCodes.Stind_I1);
                case MetadataType.Boolean: return pc.Write(OpCodes.Stind_I1);
                case MetadataType.Char: return pc.Write(OpCodes.Stind_I2);
                case MetadataType.UIntPtr: return pc.Write(OpCodes.Stind_I);
                case MetadataType.IntPtr: return pc.Write(OpCodes.Stind_I);
            }

            throw new NotSupportedException($"No instruction for {elemType.MetadataType}");
        }

        private static Cut LoadByReference(in Cut pc, TypeReference elemType)
        {
            switch (elemType.MetadataType)
            {
                case MetadataType.Class: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.Object: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.String: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.Array: return pc.Write(OpCodes.Ldind_Ref);
                case MetadataType.MVar: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.Var: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.ValueType: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.GenericInstance: return pc.Write(OpCodes.Ldobj, elemType);
                case MetadataType.Double: return pc.Write(OpCodes.Ldind_R8);
                case MetadataType.Single: return pc.Write(OpCodes.Ldind_R4);
                case MetadataType.Int64: return pc.Write(OpCodes.Ldind_I8);
                case MetadataType.UInt64: return pc.Write(OpCodes.Ldind_I8);
                case MetadataType.Int32: return pc.Write(OpCodes.Ldind_I4);
                case MetadataType.UInt32: return pc.Write(OpCodes.Ldind_U4);
                case MetadataType.Int16: return pc.Write(OpCodes.Ldind_I2);
                case MetadataType.UInt16: return pc.Write(OpCodes.Ldind_U2);
                case MetadataType.Byte: return pc.Write(OpCodes.Ldind_U1);
                case MetadataType.SByte: return pc.Write(OpCodes.Ldind_I1);
                case MetadataType.Boolean: return pc.Write(OpCodes.Ldind_U1);
                case MetadataType.Char: return pc.Write(OpCodes.Ldind_U2);
                case MetadataType.UIntPtr: return pc.Write(OpCodes.Ldind_I);
                case MetadataType.IntPtr: return pc.Write(OpCodes.Ldind_I);
            }

            throw new NotSupportedException($"No instruction for {elemType.MetadataType}");
        }

        private static Cut AttributeArgument(in Cut cut, CustomAttributeArgument argument)
        {
            var val = argument.Value;

            var pc = cut;

            if (val != null && val.GetType().IsArray)
                pc = pc.CreateArray(
                    argument.Type.GetElementType(),
                    ((Array)val).Cast<object>().Select<object, PointCut>(v => (in Cut il) => il.Value(v)).ToArray()
                    );
            else
            {
                pc = pc.Value(val);

                if (val is CustomAttributeArgument next)
                    pc = pc.Cast(next.Type, argument.Type);
            }

            return pc;
        }

        private static MethodReference GetTypeFromHandleMethod_Ref(in Cut cut)
        {
            var tref = cut.Import(StandardType.Type);
            var mr = new MethodReference("GetTypeFromHandle", tref, tref);
            mr.Parameters.Add(new ParameterDefinition(cut.Import(StandardType.RuntimeTypeHandle)));

            return mr;
        }

        private static MethodReference GetMethodFromHandleMethod_Ref(in Cut cut)
        {
            var mbref = cut.Import(StandardType.MethodBase);
            var mr = new MethodReference("GetMethodFromHandle", mbref, mbref);
            mr.Parameters.Add(new ParameterDefinition(cut.Import(StandardType.RuntimeMethodHandle)));
            mr.Parameters.Add(new ParameterDefinition(cut.Import(StandardType.RuntimeTypeHandle)));
            return mr;
        }
    }
}
