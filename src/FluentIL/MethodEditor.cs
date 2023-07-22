using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FluentIL.Extensions;
using System;
using System.Linq;

namespace FluentIL;

public static class MethodEditor
{
    public static void BeforeInstruction(this MethodDef body, Instruction instruction, PointCut action)
    {
        new Cut(body, instruction)
            .Prev()
            .Here(action);
    }

    public static void AfterInstruction(this MethodDef body, Instruction instruction, PointCut action)
    {
        new Cut(body, instruction)
            .Here(action);
    }
    public static void AfterEntry(this MethodDef body, PointCut action)
    {
        new Cut(body, GetCodeStart(body))
            .Prev()
            .Here(action);
    }

    public static void BeforeExit(this MethodDef method, PointCut action)
    {
        foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
        {
            var cut = new Cut(method, ret);
            cut.Here(action).Write(OpCodes.Ret);
            cut.Remove();
        }
    }

    public static void Append(this MethodDef method, PointCut action)
    {
        var cut = new Cut(method, entry: false, exit: true);
        cut.Here(action);
    }

    public static void Before(this MethodDef method, Instruction instruction, PointCut action)
    {
        if (!method.Body.Instructions.Contains(instruction))
            throw new ArgumentException("Wrong instruction.");

        new Cut(method, instruction)
            .SkipNops()
            .Prev()
            .Here(action);
    }

    public static void Instead(this MethodDef method, PointCut action)
    {
        method.Body.Instructions.Clear();
        method.Body.Variables.Clear();
        method.Body.ExceptionHandlers.Clear();

        new Cut(method, true, true)
            .Here(action);
    }

    public static void Mark(this MethodDef method, ITypeDefOrRef attribute)
    {
        if (method.CustomAttributes.Any(ca => ca.AttributeType.Match(attribute)))
            return;

        var constructor = ((ITypeDefOrRef)method.Module.Import(attribute)).ResolveTypeDef()
            .Methods.First(m => m.IsConstructor && !m.IsStatic);

        method.CustomAttributes.Add(new CustomAttribute(method.Module.Import(constructor)));
    }

    public static Instruction GetCodeStart(this MethodDef method)
    {
        if (method.DeclaringType.IsValueType || !method.IsConstructor || method.IsStatic)
            return method.Body.Instructions.First();

        var point = method.Body.Instructions.FirstOrDefault(
            i => i != null &&
            i.OpCode == OpCodes.Call &&
            i.Operand is IMethod methodReference &&
            IsOwnConstructor(methodReference, method.DeclaringType));

        if (point == null)
            throw new InvalidOperationException("Cannot find base class ctor call");

        return method.Body.Instructions[method.Body.Instructions.IndexOf(point) + 1];
    }

    private static bool IsOwnConstructor(IMethod methodReference, TypeDef owner)
    {
        var methodDef = methodReference.ResolveMethodDef();
        return methodDef.IsConstructor &&
            (methodDef.DeclaringType.Match(owner) || methodDef.DeclaringType.Match(owner.BaseType?.ResolveTypeDef()));
    }

    public static void OnLoadField(this MethodDef method, IField field, PointCut pc, Instruction startingFrom = null)
    {
        var fieldDef = field.ResolveFieldDef();

        if (fieldDef.IsStatic)
            method.OnEveryOccasionOf(i =>
            (i.OpCode == OpCodes.Ldsfld || i.OpCode == OpCodes.Ldsflda) && i.Operand is IField f && f.DeclaringType.Match(field.DeclaringType) && f.ResolveFieldDef() == fieldDef
            , pc, startingFrom);
        else
            method.OnEveryOccasionOf(i =>
            (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldflda) && i.Operand is IField f && f.DeclaringType.Match(field.DeclaringType) && f.ResolveFieldDef() == fieldDef
            , pc, startingFrom);
    }

    public static void OnStoreVar(this MethodDef method, Local variable, PointCut pc, Instruction startingFrom = null)
    {
        method.OnEveryOccasionOf(i =>
            ((i.OpCode == OpCodes.Stloc || i.OpCode == OpCodes.Stloc_S) && ((i.Operand is int n && n == variable.Index) || (i.Operand is Local v && v.Index == variable.Index))) ||
            (variable.Index == 0 && i.OpCode == OpCodes.Stloc_0) ||
            (variable.Index == 1 && i.OpCode == OpCodes.Stloc_1) ||
            (variable.Index == 2 && i.OpCode == OpCodes.Stloc_2) ||
            (variable.Index == 3 && i.OpCode == OpCodes.Stloc_3)
        , pc, startingFrom);
    }

    public static void OnCall(this MethodDef method, IMethod calee, PointCut pc, Instruction startingFrom = null)
    {
        var def = calee.ResolveMethodDef();

        method.OnEveryOccasionOf(i =>
            (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Calli || i.OpCode == OpCodes.Callvirt || i.OpCode == OpCodes.Newobj)
            && i.Operand is IMethod mref && mref.DeclaringType.Match(calee.DeclaringType) && mref.ResolveMethodDef() == def
        , pc, startingFrom);
    }

    public static void OnEveryOccasionOf(this MethodDef method, Func<Instruction, bool> predicate, PointCut pc, Instruction startingFrom = null)
    {
        var insts = method.Body.Instructions;
        var start = startingFrom == null ? 0 : insts.IndexOf(startingFrom);

        var icol = insts.Skip(start).Where(predicate).ToArray();

        if (icol.Length == 0)
            throw new InvalidOperationException("Expected sequence is not found even once. Unsupported language/version ?");

        foreach (var curi in icol)
            new Cut(method, curi).Here(pc);
    }
}