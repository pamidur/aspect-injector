using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentIL
{
    public static class StandardTypes
    {
        private static readonly AssemblyNameReference _corelib = ModuleDefinition.ReadModule(typeof(StandardTypes).Assembly.Location)
            .AssemblyReferences.First(r => r.Name == "netstandard");

        private static readonly ModuleDefinition _fakeModule = ModuleDefinition.CreateModule("fakemodule", ModuleKind.Dll);
        private static readonly Dictionary<Type, TypeReference> _typeCache = new Dictionary<Type, TypeReference>();      

        public static TypeReference Boolean { get; } = GetType(typeof(bool));
        public static TypeReference Byte { get; } = GetType(typeof(byte));
        public static TypeReference Char { get; } = GetType(typeof(char));
        public static TypeReference Double { get; } = GetType(typeof(double));
        public static TypeReference Int16 { get; } = GetType(typeof(short));
        public static TypeReference Int32 { get; } = GetType(typeof(int));
        public static TypeReference Int64 { get; } = GetType(typeof(long));
        public static TypeReference IntPtr { get; } = GetType(typeof(IntPtr));
        public static TypeReference Object { get; } = GetType(typeof(object));
        public static TypeReference SByte { get; } = GetType(typeof(sbyte));
        public static TypeReference Single { get; } = GetType(typeof(float));
        public static TypeReference String { get; } = GetType(typeof(string));
        public static TypeReference TypedReference { get; } = GetType(typeof(TypedReference));
        public static TypeReference UInt16 { get; } = GetType(typeof(ushort));
        public static TypeReference UInt32 { get; } = GetType(typeof(uint));
        public static TypeReference UInt64 { get; } = GetType(typeof(ulong));
        public static TypeReference UIntPtr { get; } = GetType(typeof(UIntPtr));
        public static TypeReference Void { get; } = GetType(typeof(void));
        public static TypeReference ObjectArray { get; } = new ArrayType(Object);
        public static TypeReference Task { get; } = GetType(typeof(Task));
        public static TypeReference Type { get; } = GetType(typeof(Type));
        public static TypeReference Attribute { get; } = GetType(typeof(Attribute));

        public static void UpdateCoreLibRef(ModuleDefinition module)
        {
            if (!module.AssemblyReferences.Any(r => r.Name == _corelib.Name))
                module.AssemblyReferences.Add(_corelib);
        }

        public static TypeReference GetType(Type type)
        {
            if (!_typeCache.TryGetValue(type, out var tr))
            {
                tr = new TypeReference(type.Namespace, type.Name, _fakeModule, _corelib);

                if (type.IsGenericTypeDefinition)
                {
                    var arguments = type.GetGenericArguments();
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        tr.GenericParameters.Add(new GenericParameter(arguments[i].Name, tr));
                    }
                }

                if (tr.Resolve() == null)
                    throw new InvalidOperationException($"'{type.FullName}' not a part of standard lib.");

                _typeCache[type] = tr;
            }

            return tr;
        }
    }
}