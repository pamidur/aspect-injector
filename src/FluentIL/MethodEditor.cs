using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace FluentIL
{
    public class MethodEditor
    {
        public MethodDefinition Method { get; }
        public ExtendedTypeSystem TypeSystem { get; }

        internal MethodEditor(MethodDefinition md)
        {
            Method = md;
            Method.Body?.SimplifyMacros();
            TypeSystem = md.Module.GetTypeSystem();
        }

        /// <summary>
        /// After Entry and Base Ctor Call.
        /// </summary>
        /// <param name="action"></param>
        public void AfterEntry(PointCut action)
        {
            if (!Method.HasBody) return;

            var il = Method.Body.GetEditor();
            var before = GetPreviousInst(GetCodeStart(), il);
            action(new Cut(il, before));

            if (before.OpCode == OpCodes.Nop) il.SafeRemove(before);
        }

        public void BeforeExit(PointCut action)
        {
            if (!Method.HasBody) return;

            foreach (var ret in Method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
            {
                var il = Method.Body.GetEditor();
                var nop = il.Create(OpCodes.Nop);

                il.InsertAfter(ret, il.Create(OpCodes.Ret));
                il.SafeReplace(ret, nop);

                action(new Cut(il, nop));

                il.SafeRemove(nop);
            }
        }

        public void OnException(PointCut action)
        {
        }

        public void Before(Instruction instruction, PointCut action)
        {
            if (!Method.Body.Instructions.Contains(instruction))
                throw new ArgumentException("Wrong instruction.");

            var il = Method.Body.GetEditor();
            var before = GetPreviousInst(instruction, il);

            action(new Cut(il, before));

            if (before.OpCode == OpCodes.Nop) il.SafeRemove(before);
        }

        private Instruction GetPreviousInst(Instruction instruction, ILProcessor il)
        {
            var before = instruction.Previous;
            if (before == null)
            {
                before = il.Create(OpCodes.Nop);
                il.InsertBefore(instruction, before);
            }

            return before;
        }

        public void Instead(PointCut action)
        {
            var proc = Method.Body.GetEditor();
            var instruction = proc.Create(OpCodes.Nop);

            Method.Body.Instructions.Clear();
            proc.Append(instruction);

            Before(instruction, action);

            proc.Remove(instruction);
        }

        public Instruction GetCodeStart()
        {
            var instruction = Method.IsConstructor && !Method.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            return instruction;
        }

        protected Instruction FindBaseClassCtorCall()
        {
            var proc = Method.Body.GetEditor();

            if (!Method.IsConstructor)
                throw new Exception(Method.ToString() + " is not ctor.");

            if (Method.DeclaringType.IsValueType)
                return Method.Body.Instructions.First();

            var point = Method.Body.Instructions.FirstOrDefault(
                i => i != null && i.OpCode == OpCodes.Call && i.Operand is MethodReference
                    && ((MethodReference)i.Operand).Resolve().IsConstructor
                    && (((MethodReference)i.Operand).DeclaringType.Match(Method.DeclaringType.BaseType)
                        || ((MethodReference)i.Operand).DeclaringType.Match(Method.DeclaringType)));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return point.Next;
        }

        protected Instruction GetMethodOriginalEntryPoint()
        {
            return Method.Body.Instructions.First();
        }

        public void Mark(TypeReference attribute)
        {
            if (Method.CustomAttributes.Any(ca => ca.AttributeType.FullName == attribute.FullName))
                return;

            var constructor = Method.Module.ImportReference(attribute).Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            Method.CustomAttributes.Add(new CustomAttribute(TypeSystem.Import(constructor)));
        }
    }
}