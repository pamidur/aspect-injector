using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.Core.Fluent
{
    public class MethodEditor
    {
        private readonly MethodDefinition _md;
        private readonly ExtendedTypeSystem _typeSystem;

        internal MethodEditor(MethodDefinition md)
        {
            _md = md;
            _typeSystem = md.Module.GetTypeSystem();
        }

        /// <summary>
        /// After Entry, Base Ctor Call anb before Aspect inits. Put aspect inits here.
        /// </summary>
        /// <param name="action"></param>
        public void OnInit(Action<PointCut> action)
        {
            var instruction = _md.IsConstructor && !_md.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            var proc = _md.Body.GetEditor();

            action(new PointCut(proc, instruction));
        }

        /// <summary>
        /// After Entry, Base Ctor Call and Aspect inits.
        /// </summary>
        /// <param name="action"></param>
        public void OnEntry(Action<PointCut> action)
        {
            if (!_md.HasBody) return;

            var instruction = _md.IsConstructor && !_md.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            if (_md.IsConstructor && !_md.IsStatic)
            {
                if (instruction.OpCode == OpCodes.Ldarg_0
                    && instruction.Next.OpCode == OpCodes.Call
                    && ((MethodReference)instruction.Next.Operand).Name == Constants.InstanceAspectsMethodName
                    )
                {
                    instruction = instruction.Next.Next;
                }
            }

            action(new PointCut(_md.Body.GetEditor(), instruction));
        }

        public void OnExit(Action<PointCut> action)
        {
            if (!_md.HasBody) return;

            foreach (var ret in _md.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList())
            {
                var il = _md.Body.GetEditor();
                var newRet = il.Create(OpCodes.Ret);
                var tempNop = il.Create(OpCodes.Nop);

                il.InsertAfter(ret, newRet);

                il.Replace(ret, tempNop);

                action(new PointCut(_md.Body.GetEditor(), newRet));

                il.Remove(tempNop);
            }
        }

        public void OnException(Action<PointCut> action)
        {
        }

        public void OnInstruction(Instruction instruction, Action<PointCut> action)
        {
            if (!_md.Body.Instructions.Contains(instruction))
                throw new ArgumentException("Wrong instruction.");

            action(new PointCut(_md.Body.GetEditor(), instruction));
        }

        public void Instead(Action<PointCut> action)
        {
            var proc = _md.Body.GetEditor();
            var instruction = proc.Create(OpCodes.Nop);

            _md.Body.Instructions.Clear();
            proc.Append(instruction);

            OnInstruction(instruction, action);

            proc.Remove(instruction);
        }

        protected Instruction FindBaseClassCtorCall()
        {
            var proc = _md.Body.GetEditor();

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

        protected void Mark<T>() where T : Attribute
        {
            if (_md.CustomAttributes.Any(ca => ca.AttributeType.IsTypeOf(typeof(T))))
                return;

            var constructor = _typeSystem.CompilerGeneratedAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            _md.CustomAttributes.Add(new CustomAttribute(_typeSystem.Import(constructor)));
        }
    }
}