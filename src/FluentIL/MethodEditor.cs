using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace FluentIL
{
    public static class MethodEditor
    {
        public static void AfterEntry(this MethodBody body, PointCut action)
        {
            var cut = new Cut(body, GetCodeStart(body))
                .Prev()
                .Here(action);
        }

        public static void BeforeExit(this MethodBody body, PointCut action)
        {
            foreach (var ret in body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
            {
                var cut = new Cut(body, ret);
                cut.Here(action).Write(OpCodes.Ret);
                cut.Remove();
            }
        }

        public static void Before(this MethodBody body, Instruction instruction, PointCut action)
        {
            if (!body.Instructions.Contains(instruction))
                throw new ArgumentException("Wrong instruction.");

            new Cut(body, instruction)
                .Prev()
                .Here(action);
        }

        public static void Instead(this MethodBody body, PointCut action)
        {
            body.Instructions.Clear();

            new Cut(body, true, true)
                .Here(action);
        }

        public static void Mark(this MethodDefinition method, TypeReference attribute)
        {
            if (method.CustomAttributes.Any(ca => ca.AttributeType.FullName == attribute.FullName))
                return;

            var constructor = method.Module.ImportReference(attribute).Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            method.CustomAttributes.Add(new CustomAttribute(method.Module.ImportReference(constructor)));
        }

        public static Instruction GetCodeStart(this MethodBody body)
        {
            if (body.Method.DeclaringType.IsValueType || !body.Method.IsConstructor || body.Method.IsStatic)
                return body.Method.Body.Instructions.First();

            var point = body.Instructions.FirstOrDefault(
                i => i != null && i.OpCode == OpCodes.Call && i.Operand is MethodReference
                    && ((MethodReference)i.Operand).Resolve().IsConstructor
                    && (((MethodReference)i.Operand).DeclaringType.Match(body.Method.DeclaringType.BaseType)
                        || ((MethodReference)i.Operand).DeclaringType.Match(body.Method.DeclaringType)));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return point.Next;
        }

        public static void OnLoadField(this MethodBody body, FieldReference field, PointCut pc, Instruction startingFrom = null)
        {
            var fieldDef = field.Resolve();

            if (fieldDef.IsStatic)
                body.OnEveryOccasionOf(i =>
                (i.OpCode == OpCodes.Ldsfld || i.OpCode == OpCodes.Ldsflda) && i.Operand is FieldReference f && f.DeclaringType.Match(field.DeclaringType) && f.Resolve() == fieldDef
                , pc, startingFrom);
            else
                body.OnEveryOccasionOf(i =>
                (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldflda) && i.Operand is FieldReference f && f.DeclaringType.Match(field.DeclaringType) && f.Resolve() == fieldDef
                , pc, startingFrom);
        }

        public static void OnStoreVar(this MethodBody body, VariableReference variable, PointCut pc, Instruction startingFrom = null)
        {
            body.OnEveryOccasionOf(i =>
                ((i.OpCode == OpCodes.Stloc || i.OpCode == OpCodes.Stloc_S) && ((i.Operand is int n && n == variable.Index) || (i.Operand is VariableDefinition v && v.Index == variable.Index))) ||
                (variable.Index == 0 && i.OpCode == OpCodes.Stloc_0) ||
                (variable.Index == 1 && i.OpCode == OpCodes.Stloc_1) ||
                (variable.Index == 2 && i.OpCode == OpCodes.Stloc_2) ||
                (variable.Index == 3 && i.OpCode == OpCodes.Stloc_3)
            , pc, startingFrom);
        }

        public static void OnCall(this MethodBody body, MethodReference method, PointCut pc, Instruction startingFrom = null)
        {
            body.OnEveryOccasionOf(i =>
                (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Calli || i.OpCode == OpCodes.Callvirt || i.OpCode == OpCodes.Newobj)
                && i.Operand is MethodReference mref && mref.DeclaringType.Match(method.DeclaringType) && mref.Resolve() == method.Resolve()
            , pc, startingFrom);
        }

        public static void OnEveryOccasionOf(this MethodBody body, Func<Instruction, bool> predicate, PointCut pc, Instruction startingFrom = null)
        {
            var insts = body.Instructions;
            var start = startingFrom == null ? 0 : insts.IndexOf(startingFrom);

            var icol = insts.Skip(start).ToArray();

            foreach (var curi in icol)
                if (predicate(curi))
                    new Cut(body, curi).Here(pc);
        }
    }
}