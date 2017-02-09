using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
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
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Newarr, _typeSystem.Import(elementType)));

            for (var i = 0; i < elements.Length; i++)
            {
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Dup));
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, i));

                elements[i](this);

                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Stelem_Ref));
            }

            return this;
        }

        public PointCut Call(MethodReference method, Action<PointCut> args = null)
        {
            args?.Invoke(this);

            var methodRef = _typeSystem.Import(method);// */(MethodReference)method.CreateReference(_typeSystem);
            var def = method.Resolve();

            var code = OpCodes.Call;

            if (def.IsConstructor)
                code = OpCodes.Newobj;
            else if (def.IsVirtual)
                code = OpCodes.Callvirt;

            var inst = _proc.Create(code, methodRef);
            _proc.SafeInsertBefore(_refInst, inst);

            return this;// CreatePointCut(inst);
        }

        public PointCut This()
        {
            if (_proc.Body.Method.HasThis)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldarg_0));

            return this;
        }

        public void Store(FieldReference field, Action<PointCut> val)
        {
            val(this);

            var fieldRef = _typeSystem.Import(field);// */(FieldReference)field.CreateReference(_typeSystem);
            var fieldDef = field.Resolve();

            _proc.SafeInsertBefore(_refInst, CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
        }

        public PointCut LoadAspect(AspectDefinition aspect)
        {
            FieldReference aspectField;

            if (_proc.Body.Method.IsStatic || aspect.Scope == Aspect.Scope.Global)
                aspectField = GetGlobalAspectField(aspect);
            else
            {
                aspectField = GetInstanceAspectField(aspect);
                This();
            }

            Load(aspectField);

            return this;
        }

        private FieldReference GetInstanceAspectField(AspectDefinition aspect)
        {
            var type = _proc.Body.Method.DeclaringType;

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
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldtoken, _typeSystem.Import(type)));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Call, _typeSystem.Import(_typeSystem.Type.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle"))));

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

                var ctors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in ctors)
                    ctor.GetEditor().OnInit(i => i.This().Call(instanceAspectsInitializer));
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
            var fieldRef = _typeSystem.Import(field);//*/(FieldReference)field.CreateReference(_typeSystem);
            var fieldDef = field.Resolve();

            _proc.SafeInsertBefore(_refInst, CreateInstruction(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldRef));

            return this;
        }

        public PointCut Load(ParameterReference par)
        {
            var argIndex = _proc.Body.Method.HasThis ? par.Index + 1 : par.Index;
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldarg, argIndex));
            return this;
        }

        public PointCut ByVal(TypeReference refType)
        {
            if (refType.IsByReference)
            {
                refType = ((ByReferenceType)refType).ElementType;

                if (refType.IsValueType)
                {
                    var opcode = _typeSystem.LoadIndirectMap.First(kv => refType.IsTypeOf(kv.Key)).Value;
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(opcode));
                }
                else
                    _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldind_Ref));
            }

            if (refType.IsValueType)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Box, _typeSystem.Import(refType)));

            return this;
        }

        public PointCut Value<T>(T value)
        {
            var valueType = typeof(T);

            if (valueType == typeof(bool))
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, ((bool)(object)value) ? 1 : 0));
            else if (valueType.IsValueType)
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldc_I4, (int)(object)value));
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
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldtoken, _typeSystem.Import(method)));
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Call, _typeSystem.Import(_typeSystem.MethodBase.Resolve().Methods.First(m => m.Name == "GetMethodFromHandle"))));

            return this;
        }

        public PointCut Cast(TypeReference type)
        {
            _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Castclass, _typeSystem.Import(type)));
            return this;
        }

        public Instruction CreateInstruction(OpCode opCode, int value)
        {
            return _proc.CreateOptimized(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, FieldReference value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, VariableDefinition value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, TypeReference value)
        {
            return _proc.Create(opCode, value);
        }

        public Instruction CreateInstruction(OpCode opCode, MethodReference value)
        {
            return _proc.Create(opCode, value);
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