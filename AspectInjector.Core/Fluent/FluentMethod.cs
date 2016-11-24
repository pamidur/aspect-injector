using AspectInjector.Core.Contexts;
using AspectInjector.Core.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Fluent
{
    public class FluentMethod
    {
        private readonly MethodDefinition _md;
        private readonly FluentContext _ctx;

        public string Name { get; set; }
        public MethodAttributes Attributes { get; set; }
        public TypeReference ReturnType { get; set; }
        public bool IsSpecialName { get; set; }

        public FluentMethod(FluentContext context, MethodDefinition md)
        {
        }

        public FluentMethod AddVariable(Action<VariableContructor> action)
        {
            return this;
        }

        public FluentMethod OnEntry(Action<PointCut, IEnumerable<FluentParameter>> action)
        {
        }

        public FluentMethod OnExit(Action<PointCut, IEnumerable<FluentParameter>> action)
        {
        }

        public FluentMethod OnException(Action<PointCut, IEnumerable<FluentParameter>> action)
        {
        }

        protected PointCut FindBaseClassCtorCall()
        {
            var proc = ILProcessorFactory.GetOrCreateProcessor(_md.Body);

            if (!_md.IsConstructor)
                throw new Exception(_md.ToString() + " is not ctor.");

            if (_md.DeclaringType.IsValueType)
                return new PointCut(proc, _md.Body.Instructions.First());

            var point = _md.Body.Instructions.FirstOrDefault(
                i => i != null && i.OpCode == OpCodes.Call && i.Operand is MethodReference
                    && ((MethodReference)i.Operand).Resolve().IsConstructor
                    && (((MethodReference)i.Operand).DeclaringType.IsTypeOf(_md.DeclaringType.BaseType)
                        || ((MethodReference)i.Operand).DeclaringType.IsTypeOf(_md.DeclaringType)));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return new PointCut(proc, point.Next);
        }

        protected PointCut GetMethodOriginalEntryPoint()
        {
            var processor = ILProcessorFactory.GetOrCreateProcessor(_md.Body);

            if (_md.Body.Instructions.Count == 1) //if code is optimized
                processor.InsertBefore(_md.Body.Instructions.First(), processor.Create(OpCodes.Nop));

            return new PointCut(processor, _md.Body.Instructions.First());
        }

        protected void Mark<T>(ICustomAttributeProvider member) where T : Attribute
        {
            if (member.CustomAttributes.Any(ca => ca.AttributeType.IsTypeOf(typeof(T))))
                return;

            var constructor = TypeSystem.CompilerGeneratedAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);
            _md.Module.Import
            member.CustomAttributes.Add(new CustomAttribute(_ctx.TypeSystem.Import(constructor)));
        }

        public bool SignatureMatches(FluentMethod interfaceMethod)
        {
            throw new NotImplementedException();
        }

        public void Overrides(FluentMethod interfaceMethod)
        {
            throw new NotImplementedException();
        }
    }
}