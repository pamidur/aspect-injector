using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Reflection;

namespace FluentIL
{
    public static class Values
    {
        private static readonly MethodReference _getTypeFromHandleMethod =
            StandardTypes.Type.Resolve()
            .Methods.First(m => m.Name == "GetTypeFromHandle")
            .MakeHostInstanceGeneric(StandardTypes.Type);

        private static readonly MethodReference _getMethodFromHandleMethod =
            StandardTypes.GetType(typeof(MethodBase)).Resolve()
            .Methods.First(m => m.Name == "GetMethodFromHandle" && m.Parameters.Count == 2)
            .MakeHostInstanceGeneric(StandardTypes.GetType(typeof(MethodBase)));

        public static Cut Pop(this Cut pc) => pc
            .Write(OpCodes.Pop);

        public static Cut Null(this Cut pc) => pc
            .Write(OpCodes.Ldnull);

        public static Cut Dup(this Cut pc) => pc
            .Write(OpCodes.Dup);

        public static Cut Delegate(this Cut cut, MethodReference method) => cut
            .Write(OpCodes.Ldftn, method);

        public static Cut TypeOf(this Cut pc, TypeReference type) => pc
            .Write(OpCodes.Ldtoken, pc.Method.MakeCallReference(type))
            .Write(OpCodes.Call, _getTypeFromHandleMethod);

        public static Cut MethodOf(this Cut pc, MethodReference method) => pc
            .Write(OpCodes.Ldtoken, method)
            .Write(OpCodes.Ldtoken, method.DeclaringType.MakeCallReference(method.DeclaringType))
            .Write(OpCodes.Call, _getMethodFromHandleMethod);

        public static Cut Value(this Cut pc, object value)
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
            //else if (valueType.IsArray)
            //    CreateArray(_typeSystem.Import(valueType.GetElementType()), il => ((Array)value).Cast<object>().Select(Value).ToArray());
            else
                throw new NotSupportedException(valueType.ToString());
        }

        public static Cut Primitive(this Cut pc, object value)
        {
            var valueType = value.GetType();

            switch (value)
            {
                case bool bo: return pc.Write(bo ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
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

        public static Cut Cast(this Cut cut, TypeReference typeOnStack, TypeReference expectedType)
        {
            if (typeOnStack.Match(expectedType))
                return cut;

            if (expectedType.IsByReference)
            {
                var elementType = ((ByReferenceType)expectedType).ElementType;
                cut = cut.Cast(typeOnStack, elementType);
                return StoreByReference(cut, elementType);
            }

            if (typeOnStack.IsByReference)
            {
                typeOnStack = ((ByReferenceType)typeOnStack).ElementType;
                cut = LoadByReference(cut, typeOnStack);
            }

            if (typeOnStack.Match(expectedType))
                return cut;

            if (expectedType.IsValueType || expectedType.IsGenericParameter)
            {
                if (!typeOnStack.IsValueType)
                    return cut.Write(OpCodes.Unbox_Any, expectedType);
            }
            else
            {
                if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
                    return cut.Write(OpCodes.Box, typeOnStack);
                else if (!expectedType.Match(StandardTypes.Object))
                    return cut.Write(OpCodes.Castclass, expectedType);
                else
                    return cut;
            }

            throw new Exception($"Cannot cast '{typeOnStack}' to '{expectedType}'");
        }

        private static Cut StoreByReference(Cut pc, TypeReference elemType)
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

            throw new NotSupportedException($"No instruction for {elemType.MetadataType.ToString()}");
        }

        private static Cut LoadByReference(Cut pc, TypeReference elemType)
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

            throw new NotSupportedException($"No instruction for {elemType.MetadataType.ToString()}");
        }

        private static Cut AttributeArgument(Cut pc, CustomAttributeArgument argument)
        {
            var val = argument.Value;

            if (val.GetType().IsArray)
                pc = pc.CreateArray(
                    argument.Type.GetElementType(),
                    ((Array)val).Cast<object>().Select<object, PointCut>(v => il => il.Value(v)).ToArray()
                    );
            else
            {
                pc = pc.Value(val);

                if (val is CustomAttributeArgument next)
                    pc = pc.Cast(next.Type, argument.Type);
            }

            return pc;
        }
    }
}
