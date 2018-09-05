using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class PointCut
    {
        private readonly ILProcessor _proc;
        private readonly Instruction _refInst;
        private readonly ExtendedTypeSystem _typeSystem;

        public PointCut(ILProcessor proc, Instruction instruction)
        {
            _proc = proc;
            _refInst = instruction;
            _typeSystem = proc.Body.Method.Module.GetTypeSystem();
        }

        public MethodDefinition Method { get { return _proc.Body.Method; } }

        public virtual PointCut CreatePointCut(Instruction instruction)
        {
            return new PointCut(_proc, instruction);
        }

        public void Return()
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ret));
        }

        public PointCut CreateArray<T>(params Action<PointCut>[] elements)
        {
            return CreateArray(_typeSystem.Import(typeof(T)), elements);
        }

        public PointCut CreateArray(TypeReference elementType, params Action<PointCut>[] elements)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, elements.Length));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Newarr, elementType));

            for (var i = 0; i < elements.Length; i++)
            {
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Dup));
                SetByIndex(i, elements[i]);
            }

            return this;
        }

        public PointCut Call(MethodReference method, Action<PointCut> args = null)
        {
            args?.Invoke(this);

            var methodRef = _proc.Body.Method.MakeCallReference(_typeSystem.Import(method));
            var def = method.Resolve();

            var code = OpCodes.Call;

            if (def.IsConstructor)
                code = OpCodes.Newobj;
            else if (def.IsVirtual)
                code = OpCodes.Callvirt;

            var inst = _proc.Create(code, methodRef);
            _proc.SafeInsertBefore(_refInst, inst);

            return this;
        }

        public PointCut This()
        {
            if (_proc.Body.Method.HasThis)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldarg_0));
            else throw new Exception("Attempt to load 'this' on static method.");

            return this;
        }

        public PointCut Dup()
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Dup));
            return this;
        }

        public PointCut ThisOrStatic()
        {
            if (_proc.Body.Method.HasThis)
                return This();

            return this;
        }

        public PointCut ThisOrNull()
        {
            if (_proc.Body.Method.HasThis)
                return This();
            else
                return Null();
        }

        public void Store(FieldReference field, Action<PointCut> val = null)
        {
            val?.Invoke(this);

            var fieldRef = _proc.Body.Method.MakeCallReference(_typeSystem.Import(field));
            //var fieldRef2 = _proc.Body.Method.ParametrizeGenericChild(_typeSystem.Import(field));

            var fieldDef = field.Resolve();

            _proc.SafeInsertBefore(_refInst, CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
        }

        public void Store(VariableDefinition variable, Action<PointCut> val = null)
        {
            val?.Invoke(this);
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Stloc, variable));
        }

        public void StoreByRef(ParameterReference par, Action<PointCut> val)
        {
            Load(par);
            val.Invoke(this);
            ByRef(par.ParameterType);
        }

        public PointCut LoadAspect(AspectDefinition aspect, MethodDefinition overrideTarget = null, Action<PointCut> overrideThis = null, TypeDefinition overrideSource = null)
        {
            overrideTarget = overrideTarget ?? _proc.Body.Method;
            overrideThis = overrideThis ?? (pc => pc.This());
            overrideSource = overrideSource ?? overrideTarget.DeclaringType;

            FieldReference aspectField;

            if (overrideTarget.IsStatic || aspect.Scope == Aspect.Scope.Global)
                aspectField = GetGlobalAspectField(aspect);
            else
            {
                aspectField = GetInstanceAspectField(aspect, overrideSource.Resolve());
                overrideThis(this);
            }

            Load(aspectField);

            return this;
        }

        private FieldReference GetInstanceAspectField(AspectDefinition aspect, TypeDefinition source)
        {
            var type = source;

            var fieldName = $"{Constants.AspectInstanceFieldPrefix}{aspect.Host.FullName}";

            var field = FindField(type, fieldName);
            if (field == null)
            {
                field = new FieldDefinition(fieldName, FieldAttributes.Family, _typeSystem.Import(aspect.Host));
                type.Fields.Add(field);

                InjectInitialization(GetInstanсeAspectsInitializer(type), field, aspect.CreateAspectInstance);
            }

            return field;
        }

        private FieldDefinition FindField(TypeDefinition type, string name)
        {
            if (type == null)
                return null;

            var field = type.Fields.FirstOrDefault(f => f.Name == name);
            return field ?? FindField(type.BaseType?.Resolve(), name);
        }

        private void InjectInitialization(MethodDefinition initMethod,
            FieldDefinition field,
            Action<PointCut> factory
            )
        {
            initMethod.GetEditor().OnEntry(
                e => e
                .If(
                    l => l.This().Load(field),
                    r => r.Null(),// (this.)aspect == null
                    pos => pos.This().Store(field, factory)// (this.)aspect = new aspect()
                )
            );
        }

        public PointCut TypeOf(TypeReference type)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldtoken, _proc.Body.Method.MakeCallReference(type)));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Call, _typeSystem.Type.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle")));

            return this;
        }

        private MethodDefinition GetInstanсeAspectsInitializer(TypeDefinition type)
        {
            var instanceAspectsInitializer = type.Methods.FirstOrDefault(m => m.Name == Constants.InstanceAspectsMethodName);

            if (instanceAspectsInitializer == null)
            {
                instanceAspectsInitializer = new MethodDefinition(Constants.InstanceAspectsMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig, _typeSystem.Void);

                type.Methods.Add(instanceAspectsInitializer);

                instanceAspectsInitializer.GetEditor().Instead(i => i.Return());
                instanceAspectsInitializer.GetEditor().Mark<DebuggerHiddenAttribute>();

                var ctors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.GetEditor().OnInit(i => i.This().Call(instanceAspectsInitializer.MakeHostInstanceGeneric(_proc.Body.Method.DeclaringType)));
            }

            return instanceAspectsInitializer;
        }

        private FieldReference GetGlobalAspectField(AspectDefinition aspect)
        {
            var singleton = aspect.Host.Fields.FirstOrDefault(f => f.Name == Constants.AspectGlobalField);

            if (singleton == null)
                throw new Exception("Missed aspect global singleton.");

            return singleton;
        }

        public PointCut Load(FieldReference field)
        {
            var fieldRef = _proc.Body.Method.MakeCallReference(_typeSystem.Import(field));
            var fieldDef = field.Resolve();

            _proc.SafeInsertBefore(_refInst, CreateInstruction(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldRef));

            return this;
        }

        public PointCut Load(MethodReference method)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldftn, method));
            return this;
        }

        public PointCut Load(VariableDefinition variable)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldloc, variable));
            return this;
        }

        public PointCut LoadRef(VariableDefinition variable)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldloca, variable));
            return this;
        }

        public PointCut Pop()
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Pop));

            return this;
        }

        public PointCut Load(ParameterReference par)
        {
            var argIndex = _proc.Body.Method.HasThis ? par.Index + 1 : par.Index;
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldarg, argIndex));
            return this;
        }

        public PointCut GetByIndex(int index)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, index));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldelem_Ref));
            return this;
        }

        public PointCut SetByIndex(int index, Action<PointCut> value)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, index));
            value(this);
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Stelem_Ref));
            return this;
        }

        public PointCut GetAddrByIndex(int index, TypeReference type)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, index));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldelema, type));
            return this;
        }

        public PointCut ByVal(TypeReference typeOnStack)
        {
            if (typeOnStack.IsByReference)
            {
                typeOnStack = ((ByReferenceType)typeOnStack).ElementType;

                if (typeOnStack.IsValueType)
                {
                    var opcode = _typeSystem.LoadIndirectMap.First(kv => typeOnStack.Match(kv.Key)).Value;
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(opcode));
                }
                else if (typeOnStack.IsGenericParameter)
                {
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldobj, typeOnStack));
                }
                else
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldind_Ref));
            }

            if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Box, typeOnStack));

            return this;
        }

        public PointCut ByRef(TypeReference refType)
        {
            if (refType.IsByReference)
            {
                refType = ((ByReferenceType)refType).ElementType;

                if (refType.IsValueType)
                {
                    var opcode = _typeSystem.SaveIndirectMap.First(kv => refType.Match(kv.Key)).Value;
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Unbox_Any, refType));
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(opcode));
                }
                else if (refType.IsGenericParameter)
                {
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Unbox_Any, refType));
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Stobj, refType));
                }
                else
                {
                    if (refType.FullName != WellKnownTypes.Object)
                        Cast(refType);
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Stind_Ref));
                }
            }

            return this;
        }

        public PointCut Value<T>(T value)
        {
            var valueType = typeof(T);

            if (valueType == typeof(bool))
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, ((bool)(object)value) ? 1 : 0));
            else if (valueType.IsValueType)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, (int)(object)value));
            else if (valueType == typeof(string))
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldstr, (string)(object)value));
            else if (valueType.IsClass && value == null)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldnull));
            else
                throw new NotSupportedException();

            return this;
        }

        public PointCut Null()
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldnull));
            return this;
        }

        public PointCut If(Action<PointCut> left, Action<PointCut> right, Action<PointCut> pos = null, Action<PointCut> neg = null)
        {
            left(this);
            right(this);

            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ceq));

            var continuePoint = CreateInstruction(OpCodes.Nop);
            var doIfTruePointCut = CreatePointCut(CreateInstruction(OpCodes.Nop));

            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Brfalse, continuePoint));
            _proc.SafeInsertBefore(_refInst, doIfTruePointCut._refInst);

            pos?.Invoke(doIfTruePointCut);

            if (neg != null)
            {
                var exitPoint = CreateInstruction(OpCodes.Nop);
                var doIfFlasePointCut = CreatePointCut(CreateInstruction(OpCodes.Nop));

                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Br, exitPoint));
                _proc.SafeInsertBefore(_refInst, continuePoint);
                _proc.SafeInsertBefore(_refInst, doIfFlasePointCut._refInst);

                neg(doIfFlasePointCut);

                _proc.SafeInsertBefore(_refInst, exitPoint);
            }
            else
            {
                _proc.SafeInsertBefore(_refInst, continuePoint);
            }

            return this;
        }

        public PointCut MethodOf(MethodReference method)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldtoken, method));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldtoken, method.DeclaringType.MakeCallReference(method.DeclaringType)));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Call, _typeSystem.MethodBase.Resolve().Methods.First(m => m.Name == "GetMethodFromHandle" && m.Parameters.Count == 2)));

            return this;
        }

        public PointCut Cast(TypeReference type)
        {
            if (type.IsValueType || type.IsGenericParameter)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Unbox_Any, type));
            else
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Castclass, type));

            return this;
        }

        public Instruction CreateInstruction(OpCode opCode, int value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, string value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, FieldReference value)
        {
            return _proc.Create(opCode, _typeSystem.Import(value));
        }

        public Instruction CreateInstruction(OpCode opCode, VariableDefinition value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, TypeReference value)
        {
            return _proc.Create(opCode, _typeSystem.Import(value));
        }

        public Instruction CreateInstruction(OpCode opCode, MethodReference value)
        {
            return _proc.Create(opCode, _typeSystem.Import(value));
        }

        public Instruction CreateInstruction(OpCode opCode)
        {
            return _proc.Create(opCode);
        }

        public Instruction CreateInstruction(OpCode opCode, Instruction instruction)
        {
            return _proc.Create(opCode, instruction);
        }

        public Instruction CreateInstruction(OpCode opCode, PointCut pointCut)
        {
            return _proc.Create(opCode, pointCut._refInst);
        }
    }
}