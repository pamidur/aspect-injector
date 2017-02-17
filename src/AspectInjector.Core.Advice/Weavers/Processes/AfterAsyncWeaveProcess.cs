using AspectInjector.Core.Advice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using AspectInjector.Core.Models;
using AspectInjector.Core.Extensions;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using AspectInjector.Core.Fluent;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterAsyncWeaveProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private static readonly Type[] _supportedMethodBuilders = new[] {
            typeof(AsyncTaskMethodBuilder),
            typeof(AsyncTaskMethodBuilder<>),
            typeof(AsyncVoidMethodBuilder)
        };

        private readonly TypeDefinition _stateMachine;
        private readonly FieldDefinition _this;

        public AfterAsyncWeaveProcess(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect) : base(log, target, effect, aspect)
        {
            _stateMachine = target.CustomAttributes.First(ca => ca.AttributeType.IsTypeOf(typeof(AsyncStateMachineAttribute)))
                .GetConstructorValue<TypeReference>(0).Resolve();

            _this = _target.IsStatic ? null : _stateMachine.Fields.First(f => f.IsPublic && f.FieldType.IsTypeOf(_target.DeclaringType) && f.Name.StartsWith("<>") && f.Name.EndsWith("_this"));
        }

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect, LoadThis, _target.DeclaringType)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        private void LoadThis(PointCut pc)
        {
            if (_this != null)
                pc.This().Load(_this);
        }

        protected override void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            LoadThis(pc);
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Value((object)null);
        }

        protected MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                var exitPoints = moveNext.Body.Instructions.Where(i =>
                {
                    if (i.OpCode != OpCodes.Call)
                        return false;

                    var method = ((MethodReference)i.Operand).Resolve();
                    return method.Name == "SetResult" && _supportedMethodBuilders.Any(bt => method.DeclaringType.IsTypeOf(bt));
                }).ToList();

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _ts.Void);
                _stateMachine.Methods.Add(afterMethod);

                afterMethod.GetEditor().Instead(pc => pc.Return());

                foreach (var exit in exitPoints)
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