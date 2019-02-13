using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected readonly FieldDefinition _originalThis;
        protected readonly TypeDefinition _stateMachine;

        public AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
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
                        var elements = _target.Parameters.Select<ParameterDefinition, PointCut>(p => il =>
                               il.Load(p).Cast(p.ParameterType, _ts.Object)
                           ).ToArray();

                        return v.CreateArray(_ts.Object, elements);
                    }));
            }

            return argsfield;
        }

        protected abstract void InsertStateMachineCall(PointCut code);

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().BeforeExit(
                e => e
                .LoadAspect(_aspect, _target, LoadOriginalThis)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected Cut LoadOriginalThis(Cut pc)
        {
            return _originalThis == null ? pc : pc.This().Load(_originalThis);
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
