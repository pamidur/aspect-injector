using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterIteratorWeaveProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private readonly TypeDefinition _stateMachine;
        private readonly FieldDefinition _this;

        public AfterIteratorWeaveProcess(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect)
            : base(log, target, effect, aspect)
        {
            _stateMachine = target.CustomAttributes.First(ca => ca.AttributeType.IsTypeOf(typeof(IteratorStateMachineAttribute)))
                .GetConstructorValue<TypeReference>(0).Resolve();

            var thisType = _target.DeclaringType.Resolve();

            _this = _target.IsStatic ? null : _stateMachine.Fields.First(f => f.IsPublic && f.FieldType.Resolve().IsTypeOf(thisType) && f.Name.StartsWith("<>") && f.Name.EndsWith("_this"));
        }

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect, LoadOriginalThis, _target.DeclaringType)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        private void LoadOriginalThis(PointCut pc)
        {
            if (_this != null)
                pc.This().Load(_this);
        }

        protected override void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            LoadOriginalThis(pc);
        }

        protected override void LoadReturnValueArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.This();
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            var elements = _target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
                il.ThisOrStatic().Load(FindField(p)).ByVal(p.ParameterType)
            ).ToArray();

            pc.CreateArray<object>(elements);
        }

        private FieldReference FindField(ParameterDefinition p)
        {
            return _stateMachine.Fields.First(f => f.IsPrivate && f.Name == p.Name);
        }

        protected MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                var exitPoints = moveNext.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _ts.Void);
                _stateMachine.Methods.Add(afterMethod);

                afterMethod.GetEditor().Instead(pc => pc.Return());

                foreach (var exit in exitPoints.Where(e => e.Previous.OpCode == OpCodes.Ldc_I4_0))
                {
                    moveNext.GetEditor().OnInstruction(exit, il =>
                    {
                        il.ThisOrStatic().Call(afterMethod);
                    });
                }
            }

            return afterMethod;
        }
    }
}