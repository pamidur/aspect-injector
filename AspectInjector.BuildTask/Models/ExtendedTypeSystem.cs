using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Models
{
    public class ExtendedTypeSystem
    {
        #region Fields

        private readonly ModuleDefinition _module;

        #endregion Fields

        #region Constructors

        public ExtendedTypeSystem(ModuleDefinition md)
        {
            _module = md;

            Boolean = md.TypeSystem.Boolean;
            Byte = md.TypeSystem.Byte;
            Char = md.TypeSystem.Char;
            Double = md.TypeSystem.Double;
            Int16 = md.TypeSystem.Int16;
            Int32 = md.TypeSystem.Int32;
            Int64 = md.TypeSystem.Int64;
            IntPtr = md.TypeSystem.IntPtr;
            Object = md.TypeSystem.Object;
            SByte = md.TypeSystem.SByte;
            Single = md.TypeSystem.Single;
            String = md.TypeSystem.String;
            TypedReference = md.TypeSystem.TypedReference;
            UInt16 = md.TypeSystem.UInt16;
            UInt32 = md.TypeSystem.UInt32;
            UInt64 = md.TypeSystem.UInt64;
            UIntPtr = md.TypeSystem.UIntPtr;
            Void = md.TypeSystem.Void;

            ObjectArray = MakeArrayType(Object);

            Task = md.Import(typeof(Task));
            TaskGeneric = md.Import(typeof(Task<>));
            TaskCompletionGeneric = md.Import(typeof(TaskCompletionSource<>));
            ActionGeneric = md.Import(typeof(Action<>));
            FuncGeneric = md.Import(typeof(Func<>));
            FuncGeneric2 = md.Import(typeof(Func<,>));
        }

        #endregion Constructors

        #region Properties

        public TypeReference ActionGeneric { get; private set; }
        public TypeReference Boolean { get; private set; }
        public TypeReference Byte { get; private set; }
        public TypeReference Char { get; private set; }
        public TypeReference Double { get; private set; }
        public TypeReference FuncGeneric { get; private set; }
        public TypeReference FuncGeneric2 { get; private set; }
        public TypeReference Int16 { get; private set; }
        public TypeReference Int32 { get; private set; }
        public TypeReference Int64 { get; private set; }
        public TypeReference IntPtr { get; private set; }
        public TypeReference Object { get; private set; }
        public TypeReference ObjectArray { get; private set; }
        public TypeReference SByte { get; private set; }
        public TypeReference Single { get; private set; }
        public TypeReference String { get; private set; }
        public TypeReference Task { get; private set; }
        public TypeReference TaskCompletionGeneric { get; private set; }
        public TypeReference TaskGeneric { get; private set; }
        public TypeReference TypedReference { get; private set; }
        public TypeReference UInt16 { get; private set; }
        public TypeReference UInt32 { get; private set; }
        public TypeReference UInt64 { get; private set; }
        public TypeReference UIntPtr { get; private set; }
        public TypeReference Void { get; private set; }

        #endregion Properties

        #region Methods

        public TypeReference MakeArrayType(TypeReference argument)
        {
            return _module.Import(new ArrayType(_module.Import(argument)));
        }

        public TypeReference MakeGenericInstanceType(TypeReference openGenericType, params TypeReference[] arguments)
        {
            if (openGenericType.GenericParameters.Count != arguments.Length)
                throw new ArgumentException("Generic arguments number mismatch", "arguments");

            var instance = new GenericInstanceType(openGenericType);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return _module.Import(instance);
        }

        #endregion Methods
    }
}