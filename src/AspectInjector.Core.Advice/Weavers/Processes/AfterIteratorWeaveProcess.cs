using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterIteratorWeaveProcess : AfterStateMachineWeaveProcessBase
    {       
        public AfterIteratorWeaveProcess(ILogger log, MethodDefinition target, InjectionDefinition injection)
            : base(log, target, injection)
        {
            SetStateMachine(GetStateMachine());
        }

        private TypeReference GetStateMachine()
        {
            var smRef = _method.CustomAttributes.First(ca => ca.AttributeType.Match(StandardType.IteratorStateMachineAttribute))
                .GetConstructorValue<TypeReference>(0);

            if (smRef.HasGenericParameters)
            {
                var smDef = smRef.Resolve();
                smRef = ((MethodReference)_method.Body.Instructions
                    .First(i => i.OpCode == OpCodes.Newobj && i.Operand is MemberReference mr && mr.DeclaringType.Resolve() == smDef).Operand).DeclaringType;
            }

            return smRef;
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _stateMachine.Module.TypeSystem.Void);
                _stateMachine.Methods.Add(afterMethod);
                afterMethod.Mark(_module.ImportStandardType(WellKnownTypes.DebuggerHiddenAttribute));
                afterMethod.Body.Instead((in Cut pc) => pc.Return());

                moveNext.Body.OnEveryOccasionOf(
                    i =>
                        i.Next != null && i.Next.OpCode == OpCodes.Ret &&
                        (i.OpCode == OpCodes.Ldc_I4_0 || ((i.OpCode == OpCodes.Ldc_I4 || i.OpCode == OpCodes.Ldc_I4_S) && (int)i.Operand == 0)),
                    (in Cut il) =>
                        il.ThisOrStatic().Call(afterMethod.MakeReference(_stateMachine.MakeSelfReference())));
            }

            return afterMethod;
        }


        protected override Cut LoadReturnValueArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.This();
        }

        protected override Cut LoadReturnTypeArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_stateMachine.Interfaces.First(i => i.InterfaceType.Name.StartsWith("IEnumerable`1")).InterfaceType);
        }

        protected override void InsertStateMachineCall(PointCut code)
        {
            var stateMachineCtors = _stateMachine.Methods.Where(m => m.IsConstructor).ToArray();

            foreach (var ctor in stateMachineCtors)
                _method.Body.OnCall(ctor.MakeReference(_stateMachineRef), (in Cut cut) => cut.Dup().Here(code));
        }
    }
}