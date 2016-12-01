using AspectInjector.Broker;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Models
{
    public class PointCut
    {
        private readonly ILProcessor _proc;
        private readonly Instruction _refInst;
        private readonly EditorContext _ctx;

        public PointCut(EditorContext ctx, ILProcessor proc, Instruction instruction)
        {
            _proc = proc;
            _refInst = instruction;
            _ctx = ctx;
        }

        private PointCut Chain(Instruction inst)
        {
            return CreatePointCut(inst == null ? _refInst : _proc.SafeInsertBefore(_refInst, inst));
        }

        public virtual PointCut CreatePointCut(Instruction instruction)
        {
            return new PointCut(_ctx, _proc, instruction);
        }

        public void Return(Action<PointCut> arg)
        {
            arg(Chain(CreateInstruction(OpCodes.Ret)));
        }

        public PointCut Call(MethodReference method, Action<PointCut> args = null)
        {
            args?.Invoke(this);

            var methodRef = (MethodReference)method.CreateReference(_proc.Body.Method.Module);
            var def = method.Resolve();

            var code = OpCodes.Call;

            if (def.IsConstructor)
                code = OpCodes.Newobj;
            else if (def.IsVirtual)
                code = OpCodes.Callvirt;

            var inst = _proc.Create(code, methodRef);
            _proc.SafeInsertBefore(_refInst, inst);

            return CreatePointCut(inst);
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

            var fieldRef = (FieldReference)field.CreateReference(_ctx.Module);
            var fieldDef = field.Resolve();

            _proc.SafeInsertBefore(_refInst, CreateInstruction(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, fieldRef));
        }

        public PointCut Load(Aspect aspect)
        {
            var aspectField = _ctx.Aspects.Get(aspect, _proc.Body.Method.DeclaringType);

            if (!aspectField.IsStatic)
                This();

            Load(aspectField);

            return this;
        }

        public PointCut Load(FieldReference field)
        {
            var fieldRef = (FieldReference)field.CreateReference(_ctx.Module);
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

        public PointCut ByVal(ByReferenceType refType)
        {
            var elementtype = refType.ElementType;

            if (elementtype.IsValueType)
            {
                var opcode = _ctx.TypeSystem.LoadIndirectMap.First(kv => elementtype.IsTypeOf(kv.Key)).Value;
                _proc.SafeInsertBefore(_refInst, CreateInstruction(opcode));
            }
            else
                _proc.SafeInsertBefore(_refInst, CreateInstruction(OpCodes.Ldind_Ref));

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

        public PointCut If(Action<PointCut> compare, Action<PointCut> pos = null, Action<PointCut> neg = null)
        {
            compare(this);

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