using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected readonly FieldDefinition _originalThis;
        protected readonly TypeDefinition _stateMachine;

        public AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, Injection injection) : base(log, target, injection)
        {
            _stateMachine = GetStateMachine();
            _originalThis = _target.IsStatic ? null : GetThisField();
        }

        protected abstract TypeDefinition GetStateMachine();

        private FieldDefinition GetThisField()
        {
            var thisfield = _stateMachine.Fields.FirstOrDefault(f => f.Name == Constants.MovedThis);

            if (thisfield == null)
            {
                thisfield = new FieldDefinition(Constants.MovedThis, FieldAttributes.Public, _stateMachine.MakeCallReference(_stateMachine.DeclaringType));
                _stateMachine.Fields.Add(thisfield);

                InsertStateMachineCall(
                    e => e
                    .Dup()
                    .Store(thisfield, v => v.This())
                    );
            }

            return thisfield;
        }

        private FieldDefinition GetArgsField()
        {
            var argsfield = _stateMachine.Fields.FirstOrDefault(f => f.Name == Constants.MovedArgs);

            if (argsfield == null)
            {
                argsfield = new FieldDefinition(Constants.MovedArgs, FieldAttributes.Public, _ts.ObjectArray);
                _stateMachine.Fields.Add(argsfield);

                InsertStateMachineCall(
                    e => e
                    .Dup()
                    .Store(argsfield, v =>
                    {
                        var elements = _target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
                               il.Load(p).ByVal(p.ParameterType)
                           ).ToArray();

                        v.CreateArray<object>(elements);
                    }));
            }

            return argsfield;
        }

        protected abstract void InsertStateMachineCall(Action<PointCut> code);

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect, _target, LoadOriginalThis, _target.DeclaringType)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected void LoadOriginalThis(PointCut pc)
        {
            if (_originalThis != null)
                pc.This().Load(_originalThis);
        }

        protected override void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            if (_originalThis != null)
                LoadOriginalThis(pc);
            else
                pc.Value((object)null);
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.This().Load(GetArgsField());
        }
        
        protected abstract MethodDefinition FindOrCreateAfterStateMachineMethod();
    }
}
