using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AspectInjector.BuildTask.Models
{
    public class PointCut
    {
        #region Protected Fields

        protected static readonly string RoutableAttributeVariableName = "__a$_routable_attr";

        protected readonly ILProcessor Processor;

        #endregion Protected Fields

        #region Public Constructors

        public PointCut(ILProcessor processor, Instruction instruction)
        {
            Processor = processor;
            InjectionPoint = instruction;
        }

        #endregion Public Constructors

        #region Public Properties

        public Instruction InjectionPoint { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public Instruction CreateInstruction(OpCode opCode, int value)
        {
            return Processor.CreateOptimized(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, FieldReference value)
        {
            return Processor.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, MethodReference value)
        {
            return Processor.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode)
        {
            return Processor.Create(opCode);
        }

        public Instruction CreateInstruction(OpCode opCode, Instruction instruction)
        {
            return Processor.Create(opCode, instruction);
        }

        public Instruction CreateInstruction(OpCode opCode, PointCut pointCut)
        {
            return Processor.Create(opCode, pointCut.InjectionPoint);
        }

        public virtual PointCut CreatePointCut(Instruction instruction)
        {
            return new PointCut(Processor, instruction);
        }

        public VariableDefinition CreateVariable<T>(TypeReference variableType, T defaultValue, string variableName = null)
        {
            LoadValueOntoStack(defaultValue);
            return CreateVariableFromStack(variableType, variableName);
        }

        public VariableDefinition CreateVariable(TypeReference variableType, string variableName = null)
        {
            if (!Processor.Body.InitLocals)
                Processor.Body.InitLocals = true;

            var variable = variableName == null
                ? new VariableDefinition(variableType)
                : new VariableDefinition(variableName, variableType);
            Processor.Body.Variables.Add(variable);

            return variable;
        }

        public VariableDefinition CreateVariableFromStack(TypeReference variableType, string variableName = null)
        {
            var variable = CreateVariable(variableType, variableName: variableName);
            SetVariableFromStack(variable);
            return variable;
        }

        public void Goto(Instruction instruction)
        {
            InsertBefore(Processor.Create(OpCodes.Br, instruction)); //todo:: optimize
        }

        public void Goto(PointCut pointCut)
        {
            Goto(pointCut.InjectionPoint);
        }

        public void InjectMethodCall(MethodReference method, object[] arguments)
        {
            if (method.Parameters.Count != arguments.Length)
                throw new ArgumentException("Arguments count mismatch", "arguments");

            for (int i = 0; i < method.Parameters.Count; i++)
                LoadCallArgument(arguments[i], method.Parameters[i].ParameterType);

            OpCode code;

            if (method.Resolve().IsConstructor)
                code = OpCodes.Newobj;
            else if (method.Resolve().IsVirtual)
                code = OpCodes.Callvirt;
            else
                code = OpCodes.Call;

            var methodRef = method;

            //if (methodRef.Module != Processor.Body.Method.Module || method is MethodDefinition)
            methodRef = (MethodReference)CreateMemberReference(method);

            Processor.InsertBefore(InjectionPoint, Processor.Create(code, methodRef));
        }

        public PointCut InsertAfter(Instruction instruction)
        {
            return CreatePointCut(Processor.SafeInsertAfter(InjectionPoint, instruction));
        }

        public PointCut InsertBefore(Instruction instruction)
        {
            return CreatePointCut(Processor.SafeInsertBefore(InjectionPoint, instruction));
        }

        public void LoadCallArgument(object arg, TypeReference expectedType)
        {
            var module = Processor.Body.Method.Module;

            if (arg is ParameterDefinition)
            {
                LoadParameterOntoStack((ParameterDefinition)arg, expectedType);
            }
            else if (arg is VariableDefinition)
            {
                var var = (VariableDefinition)arg;

                //if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(var.VariableType))
                //    throw new ArgumentException("Argument type mismatch");

                Processor.InsertBefore(InjectionPoint, CreateInstruction(expectedType.IsByReference ? OpCodes.Ldloca : OpCodes.Ldloc, var.Index));

                if (var.VariableType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Box, module.Import(var.VariableType)));
            }
            else if (arg is string)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(module.TypeSystem.String))
                    throw new ArgumentException("Argument type mismatch");

                var str = (string)arg;
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldstr, str));
            }
            else if (arg is CustomAttribute)
            {
                var ca = (CustomAttribute)arg;
                var catype = ca.AttributeType.Resolve();

                InjectMethodCall(ca.Constructor.Resolve(), ca.ConstructorArguments.Cast<object>().ToArray());

                if (ca.Properties.Any() || ca.Fields.Any())
                {
                    var attrvar = new VariableDefinition(RoutableAttributeVariableName, (TypeReference)CreateMemberReference(ca.AttributeType));
                    Processor.Body.Variables.Add(attrvar);
                    Processor.Body.InitLocals = true;

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

                        Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Stfld, field));
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

                LoadSelfOntoStack();
            }
            else if (arg is MethodReference)
            {
                var methodInfo = module.TypeSystem.ResolveType(typeof(MethodInfo));

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(methodInfo))
                    throw new ArgumentException("Argument type mismatch");

                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldtoken, (MethodReference)arg));
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Call, module.Import(methodInfo.Resolve().Methods.First(m => m.Name == "GetMethodFromHandle"))));
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Castclass, methodInfo));
            }
            else if (arg == Markers.DefaultMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Void))
                {
                    if (expectedType.IsValueType)
                        Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldc_I4_0));
                    else
                        Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldnull));
                }
            }
            else if (arg is TypeReference)
            {
                var typeOfType = module.TypeSystem.ResolveType(typeof(Type));

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(typeOfType))
                    throw new ArgumentException("Argument type mismatch");

                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldtoken, (TypeReference)arg));
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Call, module.Import(typeOfType.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle"))));
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

        public void LoadFieldOntoStack(FieldReference field)
        {
            var fieldRef = (FieldReference)CreateMemberReference(field);
            var fieldDef = field.Resolve();

            InsertBefore(CreateInstruction(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldRef));
        }

        public virtual void LoadParameterOntoStack(ParameterDefinition parameter, TypeReference expectedType = null)
        {
            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldarg, parameter.Index + 1));

            if (expectedType != null)
            {
                var module = Processor.Body.Method.Module;
                if (parameter.ParameterType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Box, module.Import(parameter.ParameterType)));
            }
        }

        public virtual void LoadSelfOntoStack()
        {
            Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldarg_0));
        }

        public void LoadValueOntoStack<T>(T value)
        {
            var valueType = typeof(T);

            if (valueType == typeof(bool))
                InsertBefore(Processor.CreateOptimized(OpCodes.Ldc_I4, ((bool)(object)value) ? 1 : 0));
            else if (valueType.IsValueType)
                InsertBefore(Processor.CreateOptimized(OpCodes.Ldc_I4, (int)(object)value));
            else if (valueType.IsClass && value == null)
                InsertBefore(Processor.Create(OpCodes.Ldnull));
            else
                throw new NotSupportedException();
        }

        public void LoadVariableOntoStack(VariableReference var)
        {
            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, var.Index));
        }

        public PointCut Replace(Instruction instruction)
        {
            return CreatePointCut(Processor.SafeReplace(InjectionPoint, instruction));
        }

        public void SetField(FieldReference field, Action<PointCut> loadData)
        {
            var fieldRef = (FieldReference)CreateMemberReference(field);
            var fieldDef = field.Resolve();

            InsertBefore(CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
        }

        public void SetFieldFromStack(FieldReference field)
        {
            var fieldRef = (FieldReference)CreateMemberReference(field);
            var fieldDef = field.Resolve();

            InsertBefore(CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
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

            var continuePoint = Processor.Create(OpCodes.Nop);
            var doIfTruePointCut = CreatePointCut(Processor.Create(OpCodes.Nop));

            InsertBefore(Processor.Create(OpCodes.Brfalse, continuePoint)); //todo::optimize
            InsertBefore(doIfTruePointCut.InjectionPoint);

            doIfTrue(doIfTruePointCut);

            if (doIfFalse != null)
            {
                var exitPoint = Processor.Create(OpCodes.Nop);
                var doIfFlasePointCut = CreatePointCut(Processor.Create(OpCodes.Nop));

                InsertBefore(Processor.Create(OpCodes.Br, exitPoint)); //todo::optimize
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

        public MemberReference CreateMemberReference(MemberReference member)
        {
            var module = Processor.Body.Method.Module;

            if (member is TypeReference)
            {
                if (member is IGenericParameterProvider)
                {
                    ((IGenericParameterProvider)member).GenericParameters.ToList().ForEach(tr => CreateMemberReference(tr));
                }

                if (member.Module == module || ((TypeReference)member).IsGenericParameter)
                    return member;

                return module.Import((TypeReference)member);
            }

            var declaringType = (TypeReference)CreateMemberReference(member.DeclaringType);
            var generic = member.DeclaringType as IGenericParameterProvider;

            if (generic != null && generic.HasGenericParameters)
            {
                declaringType = new GenericInstanceType((TypeReference)CreateMemberReference(member.DeclaringType));
                generic.GenericParameters.ToList()
                    .ForEach(tr => ((IGenericInstance)declaringType).GenericArguments.Add((TypeReference)CreateMemberReference(tr)));
            }

            var fieldReference = member as FieldReference;
            if (fieldReference != null)
                return new FieldReference(member.Name, (TypeReference)CreateMemberReference(fieldReference.FieldType), declaringType);

            var methodReference = member as MethodReference;
            if (methodReference != null)
            {
                //TODO: more fields may need to be copied
                var methodReferenceCopy = new MethodReference(member.Name, (TypeReference)CreateMemberReference(methodReference.ReturnType), declaringType)
                {
                    HasThis = methodReference.HasThis,
                    ExplicitThis = methodReference.ExplicitThis,
                    CallingConvention = methodReference.CallingConvention
                };

                foreach (var parameter in methodReference.Parameters)
                {
                    methodReferenceCopy.Parameters.Add(new ParameterDefinition((TypeReference)CreateMemberReference(parameter.ParameterType)));
                }

                return methodReferenceCopy;
            }

            throw new NotSupportedException("Not supported member type " + member.GetType().FullName);
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
            var module = Processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object)))
                throw new ArgumentException("Argument type mismatch");

            var parameters = ((Array)args).Cast<object>().ToArray();

            var elementType = module.Import(targetElementType.Resolve());

            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, parameters.Length));
            Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Newarr, elementType));

            if (parameters.Length > 0)
            {
                Processor.Body.InitLocals = true;

                var paramsArrayVar = new VariableDefinition(new ArrayType(elementType));
                Processor.Body.Variables.Add(paramsArrayVar);

                Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Stloc, paramsArrayVar.Index));

                for (var i = 0; i < parameters.Length; i++)
                {
                    Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, paramsArrayVar.Index));
                    Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, i));

                    LoadCallArgument(parameters[i], module.TypeSystem.Object);

                    Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Stelem_Ref));
                }

                Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldloc, paramsArrayVar.Index));
            }
        }

        private void LoadValueTypedArgument(object arg, TypeReference type, TypeReference expectedType)
        {
            if (!arg.GetType().IsValueType)
                throw new NotSupportedException("Only value types are supported.");

            var module = Processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(type))
                throw new ArgumentException("Argument type mismatch");

            if (arg is long || arg is ulong || arg is double)
            {
                var rawData = GetRawValueType(arg, 8);
                var val = BitConverter.ToInt64(rawData, 0);

                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldc_I8, val));
            }
            else
            {
                var rawData = GetRawValueType(arg, 4);
                var val = BitConverter.ToInt32(rawData, 0);

                Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, val));
            }

            if (expectedType.IsTypeOf(module.TypeSystem.Object))
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Box, module.Import(type)));
        }

        #endregion Private Methods
    }
}