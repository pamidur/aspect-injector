namespace FluentIL;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;


public delegate Cut PointCut(in Cut cut);

public static class CutEvents
{
    public static Action<CilBody> OnModify { get; set; } = m => { };
}

public readonly struct Cut
{
    private readonly bool _entry;
    private readonly bool _exit;
    public readonly Instruction ReferenceInstruction;
    private readonly CilBody _body;

    public readonly MethodDef Method;
    public ICorLibTypes TypeSystem => Method.Module.CorLibTypes;

    private IList<Instruction> Instructions => _body.Instructions;

    public Cut(MethodDef method, bool entry, bool exit)
    {
        if (!entry && !exit) throw new ArgumentException("Should be either entry or exit");

        Method = method;
        _body = method.Body;
        _entry = entry;
        _exit = exit;
        ReferenceInstruction = null;
    }

    public Cut(MethodDef method, Instruction instruction)
    {
        ReferenceInstruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
        Method = method ?? throw new ArgumentNullException(nameof(method));

        _body = method.Body;
        _entry = false;
        _exit = false;
    }

    public Cut Next()
    {
        if (_entry) return this;
        if (Instructions[Instructions.Count - 1] == ReferenceInstruction) return new Cut(Method, false, true);
        return new Cut(Method, GetNext(ReferenceInstruction));
    }

    public Cut Prev()
    {
        if (_exit) return this;
        if (Instructions.Count != 0 && Instructions[0] == ReferenceInstruction) return new Cut(Method, true, false);
        return new Cut(Method, GetPrev(ReferenceInstruction));
    }

    public Cut SkipNops()
    {
        if (_exit) return this;
        var i = _entry ? _body.Instructions[0] : ReferenceInstruction;
        while (i.OpCode == OpCodes.Nop)
            i = GetNext(i);
        return new Cut(Method, i);
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
                handler.HandlerStart = ReferenceInstruction;
        }
        else if (_exit || ReferenceInstruction == Instructions[Instructions.Count - 1])
        {
            Instructions.Add(instruction);

            if (!_exit)
                foreach (var handler in _body.ExceptionHandlers.Where(h => h.HandlerEnd == null).ToList())
                    handler.HandlerEnd = ReferenceInstruction;
        }
        else
        {
            var index = Instructions.IndexOf(ReferenceInstruction) + 1;
            Instructions.Insert(index, instruction);
        }

        return new Cut(Method, instruction);
    }

    public Cut Replace(Instruction instruction)
    {
        CutEvents.OnModify(_body);

        if (_exit || _entry) return Write(instruction);

        Redirect(ReferenceInstruction, instruction, instruction);
        Instructions[Instructions.IndexOf(ReferenceInstruction)] = instruction;

        return new Cut(Method, instruction);
    }

    public Cut Remove()
    {
        CutEvents.OnModify(_body);

        var prevCut = Prev();

        var next = GetNext(ReferenceInstruction);
        var prev = GetPrev(ReferenceInstruction);

        Redirect(ReferenceInstruction, next, prev);
        Instructions.Remove(ReferenceInstruction);

        return prevCut;
    }

    private Instruction GetNext(Instruction reference)
    {
        return _body.Instructions[_body.Instructions.IndexOf(reference) + 1];
    }

    private Instruction GetPrev(Instruction reference)
    {
        return _body.Instructions[_body.Instructions.IndexOf(reference) - 1];
    }

    private void Redirect(Instruction source, Instruction next, Instruction prev)
    {
        var refs = Instructions.Where(i => i.Operand == source).ToList();

        if (refs.Any())
        {
            if (next == null)
                throw new InvalidOperationException("Cannot redirect to non existing instruction");

            foreach (var rref in refs)
                rref.Operand = next;
        }

        foreach (var handler in _body.ExceptionHandlers)
        {
            if (handler.FilterStart == source)
                handler.FilterStart = prev ?? throw new InvalidOperationException();

            if (handler.HandlerEnd == source)
                handler.HandlerEnd = next ?? throw new InvalidOperationException();

            if (handler.HandlerStart == source)
                handler.HandlerStart = prev ?? throw new InvalidOperationException();

            if (handler.TryEnd == source)
                handler.TryEnd = next ?? throw new InvalidOperationException();

            if (handler.TryStart == source)
                handler.TryStart = prev ?? throw new InvalidOperationException();
        }
    }

}