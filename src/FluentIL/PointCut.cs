using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace FluentIL
{
    public delegate Cut PointCut(Cut cut);

    public class Cut
    {
        private readonly ILProcessor _proc;
        private readonly Instruction _refInst;

        public ExtendedTypeSystem TypeSystem { get; }
        public MethodDefinition Method { get; }

        public Cut(ILProcessor proc, Instruction instruction)
        {
            _proc = proc;
            _refInst = instruction ?? throw new ArgumentNullException(nameof(instruction));
            Method = proc.Body.Method;
            TypeSystem = Method.Module.GetTypeSystem();
        }

        public Cut Next()
        {
            return new Cut(_proc, _refInst.Next);
        }

        public Cut Prev()
        {
            return new Cut(_proc, _refInst.Previous);
        }

        public Cut Here(PointCut pc)
        {
            if (pc == null) return this;
            return pc(this);
        }

        public Cut Write(params Instruction[] instructions)
        {
            if (instructions.Length == 0)
                return this;

            var refi = _refInst;

            for (int i = 0; i < instructions.Length; i++)
            {
                Instruction inst = instructions[i];
                refi = _proc.SafeInsertAfter(refi, inst);
            }

            return new Cut(_proc, refi);
        }

        public Instruction Emit(OpCode opCode, object operand)
        {
            switch (operand)
            {
                case Cut pc: return _proc.Create(opCode, pc._refInst);
                case TypeReference tr: return _proc.Create(opCode, TypeSystem.Import(tr));
                case MethodReference mr: return _proc.Create(opCode, TypeSystem.Import(mr));
                case CallSite cs: return _proc.Create(opCode, cs);
                case FieldReference fr: return _proc.Create(opCode, TypeSystem.Import(fr));
                case string str: return _proc.Create(opCode, str);
                case char c: return _proc.Create(opCode, c);
                case byte b: return _proc.Create(opCode, b);
                case sbyte sb: return _proc.Create(opCode, sb);
                case int i: return _proc.Create(opCode, i);
                case short i: return _proc.Create(opCode, i);
                case ushort i: return _proc.Create(opCode, i);
                case long l: return _proc.Create(opCode, l);
                case float f: return _proc.Create(opCode, f);
                case double d: return _proc.Create(opCode, d);
                case Instruction inst: return _proc.Create(opCode, inst);
                case Instruction[] insts: return _proc.Create(opCode, insts);
                case VariableDefinition vd: return _proc.Create(opCode, vd);
                case ParameterDefinition pd: return _proc.Create(opCode, pd);

                default: throw new NotSupportedException($"Not supported operand type '{operand.GetType()}'");
            }
        }

        public Instruction Emit(OpCode opCode)
        {
            return _proc.Create(opCode);
        }
    }
}