using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace AspectInjector.BuildTask.Models
{
    public class PointCut
    {
        #region Private Fields

        private static readonly string RoutableAttributeVariableName = "__a$_routable_attr";

        private readonly ILProcessor _processor;

        #endregion Private Fields

        #region Public Constructors

        public PointCut(ILProcessor processor, Instruction instruction)
        {
            _processor = processor;
            InjectionPoint = instruction;
        }

        #endregion Public Constructors

        #region Public Properties

        public Instruction InjectionPoint { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public Instruction CreateInstruction(OpCode opCode, int value)
        {
            return _processor.CreateOptimized(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode)
        {
            return _processor.Create(opCode);
        }

        public Instruction CreateInstruction(OpCode opCode, Instruction instruction)
        {
            return _processor.Create(opCode, instruction);
        }

        public Instruction CreateInstruction(OpCode opCode, PointCut pointCut)
        {
            return _processor.Create(opCode, pointCut.InjectionPoint);
        }

        public VariableDefinition CreateVariable<T>(TypeReference variableType, T defaultValue, string variableName = null)
        {
            LoadValueOntoStack(defaultValue);
            return CreateVariableFromStack(variableType, variableName);
        }

        public VariableDefinition CreateVariable(TypeReference variableType, string variableName = null)
        {
            if (!_processor.Body.InitLocals)
                _processor.Body.InitLocals = true;

            var variable = variableName == null
                ? new VariableDefinition(variableType)
                : new VariableDefinition(variableName, variableType);
            _processor.Body.Variables.Add(variable);

            return variable;
        }

        public VariableDefinition CreateVariableFromStack(TypeReference variableType, string variableName = null)
        {
            var variable = CreateVariable(variableType, variableName);
            SetVariableFromStack(variable);
            return variable;
        }

        public void Goto(Instruction instruction)
        {
            InsertBefore(_processor.Create(OpCodes.Br, instruction)); //todo:: optimize
        }

        public void Goto(PointCut pointCut)
        {
            Goto(pointCut.InjectionPoint);
        }

        public void InjectMethodCall(MethodDefinition method, object[] arguments)
        {
            if (method.Parameters.Count != arguments.Length)
                throw new ArgumentException("Arguments count mismatch", "arguments");

            for (int i = 0; i < method.Parameters.Count; i++)
                LoadCallArgument(arguments[i], method.Parameters[i].ParameterType);

            OpCode code;

            if (method.IsConstructor)
                code = OpCodes.Newobj;
            else if (method.IsVirtual)
                code = OpCodes.Callvirt;
            else
                code = OpCodes.Call;

            var methodRef = (MethodReference)CreateMemberReference(method);
            _processor.InsertBefore(InjectionPoint, _processor.Create(code, methodRef));
        }

        public PointCut InsertAfter(Instruction instruction)
        {
            return new PointCut(_processor, _processor.SafeInsertAfter(InjectionPoint, instruction));
        }

        public PointCut InsertBefore(Instruction instruction)
        {
            return new PointCut(_processor, _processor.SafeInsertBefore(InjectionPoint, instruction));
        }

        public void LoadFieldOntoStack(FieldReference field)
        {
            var fieldRef = (FieldReference)CreateMemberReference(field);

            if (field.Resolve().IsStatic)
            {
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldsfld, fieldRef));
            }
            else
            {
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldarg_0));
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldfld, fieldRef));
            }
        }

        public void LoadSelfOntoStack()
        {
            _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldarg_0));
        }

        public void LoadValueOntoStack<T>(T value)
        {
            var valueType = typeof(T);

            if (valueType == typeof(bool))
                InsertBefore(_processor.CreateOptimized(OpCodes.Ldc_I4, ((bool)(object)value) ? 1 : 0));
            else if (valueType.IsValueType)
                InsertBefore(_processor.CreateOptimized(OpCodes.Ldc_I4, (int)(object)value));
            else if (valueType.IsClass && value == null)
                InsertBefore(_processor.Create(OpCodes.Ldnull));
            else
                throw new NotSupportedException();
        }

        public void LoadVariableOntoStack(VariableReference var)
        {
            _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, var.Index));
        }

        public PointCut Replace(Instruction instruction)
        {
            return new PointCut(_processor, _processor.SafeReplace(InjectionPoint, instruction));
        }

        public void SetFieldFromStack(FieldReference field)
        {
            var fieldRef = (FieldReference)CreateMemberReference(field);

            if (field.Resolve().IsStatic)
            {
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Stsfld, fieldRef));
            }
            else
            {
                var locvar = CreateVariableFromStack(field.FieldType);
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldarg_0));
                LoadVariableOntoStack(locvar);
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Stfld, fieldRef));
            }
        }

        public void SetVariable<T>(VariableDefinition variable, T value)
        {
            LoadValueOntoStack(value);
            SetVariableFromStack(variable);
        }

        public void SetVariableFromStack(VariableReference var)
        {
            InsertBefore(CreateInstruction(OpCodes.Stloc, var.Index));
        }

        public void TestValueOnStack<T>(T value, Action<PointCut> doIfTrue = null, Action<PointCut> doIfFalse = null)
        {
            LoadValueOntoStack(value);
            TestValuesOnStack(doIfTrue, doIfFalse);
        }

        public void TestValuesOnStack(Action<PointCut> doIfTrue = null, Action<PointCut> doIfFalse = null)
        {
            if (doIfTrue == null) doIfTrue = new Action<PointCut>(pc => { });
            InsertBefore(CreateInstruction(OpCodes.Ceq));

            var continuePoint = _processor.Create(OpCodes.Nop);
            var doIfTruePointCut = new PointCut(_processor, _processor.Create(OpCodes.Nop));

            InsertBefore(_processor.Create(OpCodes.Brfalse, continuePoint)); //todo::optimize
            InsertBefore(doIfTruePointCut.InjectionPoint);

            doIfTrue(doIfTruePointCut);

            if (doIfFalse != null)
            {
                var exitPoint = _processor.Create(OpCodes.Nop);
                var doIfFlasePointCut = new PointCut(_processor, _processor.Create(OpCodes.Nop));

                InsertBefore(_processor.Create(OpCodes.Br, exitPoint)); //todo::optimize
                InsertBefore(continuePoint);
                InsertBefore(doIfFlasePointCut.InjectionPoint);

                doIfFalse(doIfFlasePointCut);

                InsertBefore(exitPoint);
            }
            else
            {
                InsertBefore(continuePoint);
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected MemberReference CreateMemberReference(MemberReference member)
        {
            var module = _processor.Body.Method.Module;

            if (member is TypeReference)
            {
                return module.Import((TypeReference)member);
            }

            var declaringType = module.Import(member.DeclaringType);
            var generic = member.DeclaringType as IGenericParameterProvider;

            if (generic != null && generic.HasGenericParameters)
            {
                declaringType = new GenericInstanceType(module.Import(member.DeclaringType));
                generic.GenericParameters.ToList()
                    .ForEach(tr => ((IGenericInstance)declaringType).GenericArguments.Add(module.Import(tr)));
            }

            var fieldReference = member as FieldReference;
            if (fieldReference != null)
                return new FieldReference(member.Name, module.Import(fieldReference.FieldType), declaringType);

            var methodReference = member as MethodReference;
            if (methodReference != null)
            {
                //TODO: more fields may need to be copied
                var methodReferenceCopy = new MethodReference(member.Name, module.Import(methodReference.ReturnType), declaringType)
                {
                    HasThis = methodReference.HasThis,
                    ExplicitThis = methodReference.ExplicitThis,
                    CallingConvention = methodReference.CallingConvention
                };

                foreach (var parameter in methodReference.Parameters)
                {
                    methodReferenceCopy.Parameters.Add(new ParameterDefinition(module.Import(parameter.ParameterType)));
                }

                return methodReferenceCopy;
            }

            throw new NotSupportedException("Not supported member type " + member.GetType().FullName);
        }

        protected void LoadCallArgument(object arg, TypeReference expectedType)
        {
            var module = _processor.Body.Method.Module;

            if (arg is ParameterDefinition)
            {
                var parameter = (ParameterDefinition)arg;

                _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldarg, parameter.Index + 1));

                if (parameter.ParameterType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Box, module.Import(parameter.ParameterType)));
            }
            else if (arg is VariableDefinition)
            {
                var var = (VariableDefinition)arg;

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.Resolve().IsTypeOf(var.VariableType))
                    throw new ArgumentException("Argument type mismatch");

                _processor.InsertBefore(InjectionPoint, CreateInstruction(expectedType.IsByReference ? OpCodes.Ldloca : OpCodes.Ldloc, var.Index));

                if (var.VariableType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Box, module.Import(var.VariableType)));
            }
            else if (arg is string)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(module.TypeSystem.String))
                    throw new ArgumentException("Argument type mismatch");

                var str = (string)arg;
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldstr, str));
            }
            else if (arg is CustomAttribute)
            {
                var ca = (CustomAttribute)arg;
                var catype = ca.AttributeType.Resolve();

                InjectMethodCall(ca.Constructor.Resolve(), ca.ConstructorArguments.Cast<object>().ToArray());

                if (ca.Properties.Any() || ca.Fields.Any())
                {
                    var attrvar = new VariableDefinition(RoutableAttributeVariableName, (TypeReference)CreateMemberReference(ca.AttributeType));
                    _processor.Body.Variables.Add(attrvar);
                    _processor.Body.InitLocals = true;

                    SetVariableFromStack(attrvar);

                    foreach (var namedArg in ca.Properties)
                    {
                        LoadVariableOntoStack(attrvar);
                        InjectMethodCall(catype.Properties.First(p => p.Name == namedArg.Name).SetMethod, new object[] { namedArg.Argument });
                    }

                    foreach (var namedArg in ca.Fields)
                    {
                        LoadVariableOntoStack(attrvar);

                        var field = catype.Fields.First(p => p.Name == namedArg.Name);
                        LoadCallArgument(namedArg.Argument, field.FieldType);

                        _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Stfld, field));
                    }

                    LoadVariableOntoStack(attrvar);
                }
            }
            else if (arg is CustomAttributeArgument)
            {
                var caa = (CustomAttributeArgument)arg;

                if (caa.Type.IsArray)
                {
                    if (!expectedType.IsTypeOf(module.TypeSystem.Object) &&
                        !expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object)))
                        throw new ArgumentException("Argument type mismatch");

                    LoadArray(caa.Value, caa.Type.GetElementType(), expectedType);
                }
                else if (caa.Value is CustomAttributeArgument || caa.Type.IsTypeOf(module.TypeSystem.String))
                {
                    LoadCallArgument(caa.Value, expectedType);
                }
                else
                {
                    LoadValueTypedArgument(caa.Value, caa.Type, expectedType);
                }
            }
            else if (arg == Markers.InstanceSelfMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object))
                    throw new ArgumentException("Argument type mismatch");

                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldarg_0));
            }
            else if (arg == Markers.DefaultMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Void))
                {
                    if (expectedType.IsValueType)
                        _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldc_I4_0));
                    else
                        _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldnull));
                }
            }
            else if (arg is TypeReference)
            {
                var typeOfType = module.TypeSystem.ResolveType(typeof(Type));

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(typeOfType))
                    throw new ArgumentException("Argument type mismatch");

                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldtoken, (TypeReference)arg));
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Call, module.Import(typeOfType.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle"))));
            }
            else if (arg.GetType().IsValueType)
            {
                var type = module.TypeSystem.ResolveType(arg.GetType());
                LoadValueTypedArgument(arg, type, expectedType);
            }
            else if (arg is Array)
            {
                var elementType = arg.GetType().GetElementType();

                if (elementType == typeof(ParameterDefinition))
                    elementType = typeof(object);

                LoadArray(arg, module.TypeSystem.ResolveType(elementType), expectedType);
            }
            else
            {
                throw new NotSupportedException("Argument type of " + arg.GetType().ToString() + " is not supported");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private byte[] GetRawValueType(object value, int @base = 0)
        {
            byte[] rawData = new byte[@base == 0 ? Marshal.SizeOf(value) : @base];

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return rawData;
        }

        private void LoadArray(object args, TypeReference targetElementType, TypeReference expectedType)
        {
            var module = _processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object)))
                throw new ArgumentException("Argument type mismatch");

            var parameters = ((Array)args).Cast<object>().ToArray();

            var elementType = module.Import(targetElementType.Resolve());

            _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, parameters.Length));
            _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Newarr, elementType));

            if (parameters.Length > 0)
            {
                _processor.Body.InitLocals = true;

                var paramsArrayVar = new VariableDefinition(new ArrayType(elementType));
                _processor.Body.Variables.Add(paramsArrayVar);

                _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Stloc, paramsArrayVar.Index));

                for (var i = 0; i < parameters.Length; i++)
                {
                    _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, paramsArrayVar.Index));
                    _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, i));

                    LoadCallArgument(parameters[i], module.TypeSystem.Object);

                    _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Stelem_Ref));
                }

                _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, paramsArrayVar.Index));
            }
        }

        private void LoadValueTypedArgument(object arg, TypeReference type, TypeReference expectedType)
        {
            if (!arg.GetType().IsValueType)
                throw new NotSupportedException("Only value types are supported.");

            var module = _processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(type))
                throw new ArgumentException("Argument type mismatch");

            if (arg is long || arg is ulong || arg is double)
            {
                var rawData = GetRawValueType(arg, 8);
                var val = BitConverter.ToInt64(rawData, 0);

                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Ldc_I8, val));
            }
            else
            {
                var rawData = GetRawValueType(arg, 4);
                var val = BitConverter.ToInt32(rawData, 0);

                _processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, val));
            }

            if (expectedType.IsTypeOf(module.TypeSystem.Object))
                _processor.InsertBefore(InjectionPoint, _processor.Create(OpCodes.Box, module.Import(type)));
        }

        #endregion Private Methods
    }
}