using AspectInjector.Core.Contexts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Fluent
{
    public class MethodEditor
    {
        private readonly MethodDefinition _md;
        private readonly EditorContext _ctx;

        public string Name { get; set; }
        public MethodAttributes Attributes { get; set; }
        public TypeReference ReturnType { get; set; }
        public bool IsSpecialName { get; set; }

        public ExtendedTypeSystem TypeSystem { get; private set; }

        internal MethodEditor(EditorContext context, MethodDefinition md)
        {
            _ctx = context;
            _md = md;
        }

        public void OnInit(Action<PointCut> action)
        {
            var instruction = _md.IsConstructor && !_md.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            var proc = _ctx.Factory.GetProcessor(_md.Body);

            if (instruction.OpCode != OpCodes.Nop) //add nop
                instruction = proc.SafeInsertBefore(instruction, proc.Create(OpCodes.Nop));

            action(new PointCut(_ctx, proc, instruction));
        }

        public void OnEntry(Action<PointCut> action)
        {
            var instruction = _md.IsConstructor && !_md.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            while (instruction.OpCode == OpCodes.Nop) //skip all nops
                instruction = instruction.Next;

            action(new PointCut(_ctx, _ctx.Factory.GetProcessor(_md.Body), instruction));
        }

        public void OnExit(Action<PointCut> action)
        {
        }

        public void OnException(Action<PointCut> action)
        {
        }

        public void OnInstruction(Instruction instruction, Action<PointCut> action)
        {
            if (!_md.Body.Instructions.Contains(instruction))
                throw new ArgumentException("Wrong instruction.");

            action(new PointCut(_ctx, _ctx.Factory.GetProcessor(_md.Body), instruction));
        }

        public void Instead(Action<PointCut> action)
        {
            var proc = _ctx.Factory.GetProcessor(_md.Body);
            var instruction = proc.Create(OpCodes.Nop);

            _md.Body.Instructions.Clear();
            proc.Append(instruction);

            OnInstruction(instruction, action);

            proc.Remove(instruction);
        }

        protected Instruction FindBaseClassCtorCall()
        {
            var proc = _ctx.Factory.GetProcessor(_md.Body);

            if (!_md.IsConstructor)
                throw new Exception(_md.ToString() + " is not ctor.");

            if (_md.DeclaringType.IsValueType)
                return _md.Body.Instructions.First();

            var point = _md.Body.Instructions.FirstOrDefault(
                i => i != null && i.OpCode == OpCodes.Call && i.Operand is MethodReference
                    && ((MethodReference)i.Operand).Resolve().IsConstructor
                    && (((MethodReference)i.Operand).DeclaringType.IsTypeOf(_md.DeclaringType.BaseType)
                        || ((MethodReference)i.Operand).DeclaringType.IsTypeOf(_md.DeclaringType)));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return point.Next;
        }

        protected Instruction GetMethodOriginalEntryPoint()
        {
            return _md.Body.Instructions.First();
        }

        protected void Mark<T>(ICustomAttributeProvider member) where T : Attribute
        {
            if (member.CustomAttributes.Any(ca => ca.AttributeType.IsTypeOf(typeof(T))))
                return;

            var constructor = TypeSystem.CompilerGeneratedAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            member.CustomAttributes.Add(new CustomAttribute(_md.Module.Import(constructor)));
        }

        public bool SignatureMatches(MethodEditor interfaceMethod)
        {
            throw new NotImplementedException();
        }

        public void Overrides(MethodEditor interfaceMethod)
        {
            throw new NotImplementedException();
        }
    }
}