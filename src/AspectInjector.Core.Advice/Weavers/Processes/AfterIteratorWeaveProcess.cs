using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterIteratorWeaveProcess : AfterStateMachineWeaveProcessBase
    {
        private static readonly TypeReference _iteratorStateMachineAttribute = StandardTypes.GetType(typeof(IteratorStateMachineAttribute));

        public AfterIteratorWeaveProcess(ILogger log, MethodDefinition target, InjectionDefinition injection)
            : base(log, target, injection)
        {

        }

        protected override TypeReference GetStateMachine()
        {
            var smRef = _target.CustomAttributes.First(ca => ca.AttributeType.Match(_iteratorStateMachineAttribute))
                .GetConstructorValue<TypeReference>(0);

            if (smRef.HasGenericParameters)
            {
                var smDef = smRef.Resolve();
                smRef = ((MethodReference)_target.Body.Instructions
                    .First(i => i.OpCode == OpCodes.Newobj && i.Operand is MemberReference mr && mr.DeclaringType.Resolve() == smDef).Operand).DeclaringType;
            }

            //smRef = smRef.MakeGenericInstanceType(_target.DeclaringType.GenericParameters.Concat(_target.GenericParameters).ToArray());

            return smRef;
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _stateMachine.Module.ImportReference(StandardTypes.Void));
                _stateMachine.Methods.Add(afterMethod);
                afterMethod.Mark(WellKnownTypes.DebuggerHiddenAttribute);
                afterMethod.Body.Instead(pc => pc.Return());

                moveNext.Body.OnEveryOccasionOf(
                    i =>
                        i.Next != null && i.Next.OpCode == OpCodes.Ret &&
                        (i.OpCode == OpCodes.Ldc_I4_0 || ((i.OpCode == OpCodes.Ldc_I4 || i.OpCode == OpCodes.Ldc_I4_S) && (int)i.Operand == 0)),
                    il =>
                        il.ThisOrStatic().Call(afterMethod.MakeGenericReference(_stateMachine)));
            }

            return afterMethod;
        }


        protected override Cut LoadReturnValueArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.This();
        }

        protected override Cut LoadReturnTypeArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_stateMachine.Interfaces.First(i => i.InterfaceType.Name.StartsWith("IEnumerable`1")).InterfaceType);
        }

        protected override void InsertStateMachineCall(PointCut code)
        {
            var stateMachineCtors = _stateMachine.Methods.Where(m => m.IsConstructor).ToArray();

            foreach (var ctor in stateMachineCtors)
                _target.Body.OnCall(ctor.MakeGenericReference(_stateMachineRef), cut => cut.Dup().Here(code));
        }
    }
}