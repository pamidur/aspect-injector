using Mono.Cecil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluentIL
{
    public class ExtendedTypeSystem
    {
        #region Private Fields

        private readonly ModuleDefinition _module;

        private static readonly AssemblyNameReference _corelib;

        static ExtendedTypeSystem()
        {
            var netStandard = ModuleDefinition.ReadModule(typeof(ExtendedTypeSystem).Assembly.Location).AssemblyReferences.First(r => r.Name == "netstandard");
            _corelib = netStandard;
        }


        #endregion Private Fields

        #region Public Constructors

        public ExtendedTypeSystem(ModuleDefinition md)
        {
            _module = md;

            UpdateCoreLibRef(_module, _corelib);

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

            Task = GetSystemType(typeof(Task));
            TaskGeneric = GetSystemType(typeof(Task<>));
            TaskCompletionGeneric = GetSystemType(typeof(TaskCompletionSource<>));
            ActionGeneric = GetSystemType(typeof(Action<>));
            FuncGeneric = GetSystemType(typeof(Func<>));
            FuncGeneric2 = GetSystemType(typeof(Func<,>));

            MethodBase = GetSystemType(typeof(MethodBase));
            Type = GetSystemType(typeof(Type));

            Attribute = GetSystemType(typeof(Attribute));

            DebuggerHiddenAttribute = GetSystemType(typeof(DebuggerHiddenAttribute));
            DebuggerStepThroughAttribute = GetSystemType(typeof(DebuggerStepThroughAttribute));
            CompilerGeneratedAttribute = GetSystemType(typeof(CompilerGeneratedAttribute));

            IteratorStateMachineAttribute = GetSystemType(typeof(IteratorStateMachineAttribute));
            AsyncStateMachineAttribute = GetSystemType(typeof(AsyncStateMachineAttribute));

            GetTypeFromHandleMethod = Type.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle");
            GetMethodFromHandleMethod = MethodBase.Resolve().Methods.First(m => m.Name == "GetMethodFromHandle" && m.Parameters.Count == 2);
        }

        private static void UpdateCoreLibRef(ModuleDefinition module, AssemblyNameReference reference)
        {
            if (!module.AssemblyReferences.Any(r => r.Name == reference.Name))
                module.AssemblyReferences.Add(reference);
        }

        private TypeReference GetSystemType(Type type)
        {
            var tr = new TypeReference(type.Namespace, type.Name, _module, _corelib);

            if (type.IsGenericTypeDefinition)
            {
                var arguments = type.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    tr.GenericParameters.Add(new GenericParameter(arguments[i].Name, tr));
                }
            }

            return tr;
        }

        internal FieldReference Import(FieldReference field)
        {
            //IGenericParameterProvider context = null;
            //if (field.DeclaringType.IsGenericParameter)
            //    context = ((GenericParameter)field.DeclaringType).Owner;

            return _module.ImportReference(field/*, context*/);
        }

        public ModuleDefinition GetModule()
        {
            return _module;
        }

        public TypeReference Import(TypeReference type, IGenericParameterProvider genericContext)
        {
            return _module.ImportReference(type, genericContext);
        }

        public TypeReference Import(TypeReference type)
        {
            IGenericParameterProvider context = null;
            //if (type.IsGenericParameter)
            //    context = ((GenericParameter)type).Owner;

            return _module.ImportReference(type, context);
        }

        public TypeReference Import(Type type)
        {
            IGenericParameterProvider context = null;
            //if (type.IsGenericParameter)
            //    context = ((GenericParameter)type).Owner;

            return _module.ImportReference(type, context);
        }

        public MethodReference Import(MethodReference method)
        {
            return _module.ImportReference(method);
        }

        #endregion Public Constructors

        #region Public Properties

        public TypeReference ActionGeneric { get; private set; }
        public TypeReference Boolean { get; private set; }
        public TypeReference Byte { get; private set; }
        public TypeReference Char { get; private set; }

        public TypeReference Attribute { get; private set; }
        public TypeReference CompilerGeneratedAttribute { get; private set; }
        public TypeReference IteratorStateMachineAttribute { get; private set; }
        public TypeReference AsyncStateMachineAttribute { get; private set; }
        public TypeReference DebuggerHiddenAttribute { get; internal set; }
        public TypeReference DebuggerStepThroughAttribute { get; private set; }

        public TypeReference Double { get; private set; }
        public TypeReference FuncGeneric { get; private set; }
        public TypeReference FuncGeneric2 { get; private set; }
        public TypeReference Int16 { get; private set; }
        public TypeReference Int32 { get; private set; }
        public TypeReference Int64 { get; private set; }
        public TypeReference IntPtr { get; private set; }
        public TypeReference MethodBase { get; private set; }
        public TypeReference Object { get; private set; }
        public TypeReference ObjectArray { get; private set; }

        public MethodReference GetTypeFromHandleMethod { get; private set; }
        public MethodReference GetMethodFromHandleMethod { get; private set; }

        public TypeReference SByte { get; private set; }
        public TypeReference Single { get; private set; }
        public TypeReference String { get; private set; }
        public TypeReference Task { get; private set; }

        public TypeReference TaskCompletionGeneric { get; private set; }
        public TypeReference TaskGeneric { get; private set; }
        public TypeReference Type { get; private set; }
        public TypeReference TypedReference { get; private set; }
        public TypeReference UInt16 { get; private set; }
        public TypeReference UInt32 { get; private set; }
        public TypeReference UInt64 { get; private set; }
        public TypeReference UIntPtr { get; private set; }
        public TypeReference Void { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public TypeReference MakeArrayType(TypeReference argument)
        {
            return Import(new ArrayType(Import(argument)));
        }

        public TypeReference MakeGenericInstanceType(TypeReference openGenericType, params TypeReference[] arguments)
        {
            if (openGenericType.GenericParameters.Count != arguments.Length)
                throw new ArgumentException("Generic arguments number mismatch", "arguments");

            var instance = new GenericInstanceType(openGenericType);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return Import(instance);
        }

        #endregion Public Methods
    }
}