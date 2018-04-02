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
    internal class AfterIteratorWeaveProcess : AfterStateMachineWeaveProcessBase
    {
        public AfterIteratorWeaveProcess(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect)
            : base(log, target, effect, aspect)
        {

        }

        protected override TypeDefinition GetStateMachine()
        {
            return _target.CustomAttributes.First(ca => ca.AttributeType.IsTypeOf(typeof(IteratorStateMachineAttribute)))
                .GetConstructorValue<TypeReference>(0).Resolve();
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                var moveNextEditor = moveNext.GetEditor();

                var exitPoints = moveNext.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _ts.Void);
                _stateMachine.Methods.Add(afterMethod);

                afterMethod.GetEditor().Instead(pc => pc.Return());

                foreach (var exit in exitPoints.Where(e => e.Previous.OpCode == OpCodes.Ldc_I4 && (int)e.Previous.Operand == 0))
                {
                    moveNextEditor.OnInstruction(exit, il =>
                    {
                        il.ThisOrStatic().Call(afterMethod.MakeHostInstanceGeneric(_stateMachine));
                    });
                }
            }

            return afterMethod;
        }


        protected override void LoadReturnValueArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.This();
        }

        protected override void LoadReturnTypeArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.TypeOf(_stateMachine.Interfaces.First(i => i.Name.StartsWith("IEnumerable`1")));
        }

        protected override void InsertStateMachineCall(Action<PointCut> code)
        {
            _target.GetEditor().OnExit(code);
        }
    }
}