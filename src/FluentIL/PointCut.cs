using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Linq;

namespace FluentIL
{
    public delegate Cut PointCut(Cut cut);

    public static class CutEvents
    {
        public static Action<MethodBody> OnModify = m => { };
    }

    public struct Cut
    {
        private readonly bool _entry;
        private readonly bool _exit;
        private readonly Instruction _refInst;
        private readonly MethodBody _body;

        public MethodDefinition Method => _body.Method;

        private Collection<Instruction> Instructions => _body.Instructions;

        public Cut(MethodBody body, bool entry, bool exit)
        {
            if (!entry && !exit) throw new ArgumentException();

            _body = body;
            _entry = entry;
            _exit = exit;
            _refInst = null;
        }

        public Cut(MethodBody body, Instruction instruction)
        {
            _refInst = instruction ?? throw new ArgumentNullException(nameof(instruction));
            _body = body ?? throw new ArgumentNullException(nameof(body));

            _entry = false;
            _exit = false;           
        }

        public Cut Next()
        {
            if (_entry) return this;
            if (Instructions[Instructions.Count - 1] == _refInst) return new Cut(_body, false, true);
            return new Cut(_body, _refInst.Next);
        }

        public Cut Prev()
        {
            if (_exit) return this;
            if (Instructions.Count != 0 && Instructions[0] == _refInst) return new Cut(_body, true, false);
            return new Cut(_body, _refInst.Previous);
        }

        public Cut Here(PointCut pc)
        {
            if (pc == null) return this;
            return pc(this);
        }

        public Cut Write(Instruction instruction)
        {
            CutEvents.OnModify(_body);

            if (_entry)
            {
                Instructions.Insert(0, instruction);

                foreach (var handler in _body.ExceptionHandlers.Where(h => h.HandlerStart == null).ToList())
                    handler.HandlerStart = _refInst;
            }
            else if (_exit || _refInst == Instructions[Instructions.Count - 1])
            {
                Instructions.Add(instruction);

                if (!_exit)
                    foreach (var handler in _body.ExceptionHandlers.Where(h => h.HandlerEnd == null).ToList())
                        handler.HandlerEnd = _refInst;
            }
            else
                Instructions.Insert(Instructions.IndexOf(_refInst) + 1, instruction);            

            return new Cut(_body, instruction);
        }

        public Instruction Emit(OpCode opCode, object operand)
        {
            switch (operand)
            {                
                case Cut pc: return Instruction.Create(opCode, pc._refInst ?? throw new InvalidOperationException());
                case TypeReference tr: return Instruction.Create(opCode, Method.Module.ImportReference(tr));
                case MethodReference mr: return Instruction.Create(opCode, Method.Module.ImportReference(mr));
                case CallSite cs: return Instruction.Create(opCode, cs);
                case FieldReference fr: return Instruction.Create(opCode, Method.Module.ImportReference(fr));
                case string str: return Instruction.Create(opCode, str);
                case char c: return Instruction.Create(opCode, c);
                case byte b: return Instruction.Create(opCode, b);
                case sbyte sb: return Instruction.Create(opCode, sb);
                case int i: return Instruction.Create(opCode, i);
                case short i: return Instruction.Create(opCode, i);
                case ushort i: return Instruction.Create(opCode, i);
                case long l: return Instruction.Create(opCode, l);
                case float f: return Instruction.Create(opCode, f);
                case double d: return Instruction.Create(opCode, d);
                case Instruction inst: return Instruction.Create(opCode, inst);
                case Instruction[] insts: return Instruction.Create(opCode, insts);
                case VariableDefinition vd: return Instruction.Create(opCode, vd);
                case ParameterDefinition pd: return Instruction.Create(opCode, pd);

                default: throw new NotSupportedException($"Not supported operand type '{operand.GetType()}'");
            }
        }

        public Instruction Emit(OpCode opCode)
        {
            return Instruction.Create(opCode);
        }

        public Cut Replace(Instruction instruction)
        {
            CutEvents.OnModify(_body);

            if (_exit || _entry) return Write(instruction);

            Redirect(_refInst, instruction, instruction);
            Instructions[Instructions.IndexOf(_refInst)] = instruction;

            return new Cut(_body, instruction);
        }

        public Cut Remove()
        {
            CutEvents.OnModify(_body);

            var prevCut = Prev();

            var next = _refInst.Next;
            var prev = _refInst.Previous;

            Redirect(_refInst, next, prev);
            Instructions.Remove(_refInst);

            return prevCut;
        }

        private void Redirect(Instruction source, Instruction to, Instruction from)
        {
            var refs = Instructions.Where(i => i.Operand == source).ToList();

            if (refs.Any())
            {
                if (to == null)
                    throw new InvalidOperationException();

                foreach (var rref in refs)
                    rref.Operand = to;
            }

            foreach (var handler in _body.ExceptionHandlers)
            {
                if (handler.FilterStart == source)
                    handler.FilterStart = to ?? throw new InvalidOperationException();

                if (handler.HandlerEnd == source)
                    handler.HandlerEnd = from ?? throw new InvalidOperationException();

                if (handler.HandlerStart == source)
                    handler.HandlerStart = to ?? throw new InvalidOperationException();

                if (handler.TryEnd == source)
                    handler.TryEnd = from ?? throw new InvalidOperationException();

                if (handler.TryStart == source)
                    handler.TryStart = to ?? throw new InvalidOperationException();
            }
        }

    }
}