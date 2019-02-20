using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected readonly TypeDefinition _stateMachine;
        private readonly Func<FieldDefinition> _originalThis;
        protected readonly TypeReference _stateMachineRef;

        public AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _stateMachineRef = GetStateMachine();
            _stateMachine = _stateMachineRef.Resolve();
            _originalThis = _target.IsStatic ? (Func<FieldDefinition>) null : () => GetThisField();
        }

        protected abstract TypeReference GetStateMachine();

        private FieldDefinition GetThisField()
        {
            var thisfield = _stateMachine.Fields.FirstOrDefault(f => f.Name == Constants.MovedThis);

            if (thisfield == null)
            {
                thisfield = new FieldDefinition(Constants.MovedThis, FieldAttributes.Public, _stateMachine.MakeCallReference(_stateMachine.DeclaringType));
                _stateMachine.Fields.Add(thisfield);

                InsertStateMachineCall(
                    e => e
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
                argsfield = new FieldDefinition(Constants.MovedArgs, FieldAttributes.Public, _stateMachine.Module.ImportReference(StandardTypes.ObjectArray));
                _stateMachine.Fields.Add(argsfield);

                InsertStateMachineCall(
                    e => e
                    .Store(argsfield, v =>
                    {
                        var elements = _target.Parameters.Select<ParameterDefinition, PointCut>(p => il =>
                               il.Load(p).Cast(p.ParameterType, StandardTypes.Object)
                           ).ToArray();

                        return v.CreateArray(StandardTypes.Object, elements);
                    }));
            }

            return argsfield;
        }

        protected abstract void InsertStateMachineCall(PointCut code);

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().Body.BeforeExit(
                e => e
                .LoadAspect(_aspect, _target, LoadOriginalThis)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected Cut LoadOriginalThis(Cut pc)
        {
            return _originalThis == null ? pc : pc.This().Load(_originalThis());
        }

        protected override Cut LoadInstanceArgument(Cut pc, AdviceArgument parameter)
        {
            if (_originalThis != null)
                return LoadOriginalThis(pc);
            else
                return pc.Value(null);
        }

        protected override Cut LoadArgumentsArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.This().Load(GetArgsField());
        }

        protected abstract MethodDefinition FindOrCreateAfterStateMachineMethod();
    }
}
